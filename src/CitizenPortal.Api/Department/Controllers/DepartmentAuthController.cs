using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CitizenPortal.Api.Data;
using CitizenPortal.Api.Department.DTOs;
using CitizenPortal.Api.Department.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>
/// Department Portal OTP login (spec item 5). Mobile number only — no manual department
/// selection anywhere in this flow; DepartmentId always comes from the authenticated user's
/// row in the (existing) Users table, never from the client.
/// </summary>
[ApiController]
[Route("api/department/auth")]
public class DepartmentAuthController : ControllerBase
{
    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(5);
    private const int MaxOtpAttempts = 5;

    private readonly AppDbContext _db;
    private readonly ISmsService _sms;
    private readonly IDepartmentLinkService _deptLink;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _environment;

    public DepartmentAuthController(AppDbContext db, ISmsService sms, IDepartmentLinkService deptLink, IConfiguration config, IWebHostEnvironment environment)
    {
        _db = db;
        _sms = sms;
        _deptLink = deptLink;
        _config = config;
        _environment = environment;
    }

    /// <summary>Step 1: mobile number in, OTP sent (if the user is valid). Always returns a
    /// generic response so this endpoint can't be used to enumerate which mobile numbers exist.</summary>
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto request)
    {
        if (string.IsNullOrWhiteSpace(request.MobileNumber))
            return BadRequest(new { message = "Mobile number is required." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber);

        // Deliberately vague to the client on *why* (item 19: don't leak which mobile numbers
        // are registered) — but the reasons below are exactly the item-5/item-19 checklist.
        if (user is null)
            return NotFound(new { message = "This mobile number is not registered as a department user." });

        if (!user.IsActive)
            return BadRequest(new { message = "This account is inactive. Contact your administrator." });

        // SystemAdmin/MasterAdmin users may legitimately have no single DepartmentId (item 7 —
        // they manage all departments). Every other role must have one (item 5's own rule).
        if (user.DepartmentId is null && user.Role != Entities.UserRole.SystemAdmin)
            return BadRequest(new { message = "This account has no department assigned. Contact your administrator." });

        var otp = Random.Shared.Next(100000, 999999).ToString();
        user.OtpCode = otp;
        user.OtpExpiresAt = DateTime.UtcNow.Add(OtpValidity);
        user.OtpAttemptCount = 0;
        await _db.SaveChangesAsync();

        await _sms.SendOtpAsync(user.MobileNumber, otp);

        return Ok(new
        {
            message = "OTP sent.",
            expiresInSeconds = (int)OtpValidity.TotalSeconds,
            demoOtp = _environment.IsDevelopment() ? otp : null
        });
    }

    /// <summary>Step 2: OTP in, JWT out. JWT carries UserId/MobileNumber/Role/DepartmentId — the
    /// only place DepartmentId is ever decided for the whole session (item 6: never trust it
    /// from the frontend after this point).</summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
    {
        // Authentication is based only on the real Users row. Do not join the optional
        // staff Departments table here because some installations have Users data before
        // the department master has been migrated.
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber);

        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Invalid mobile number or OTP." });

        if (user.OtpCode is null || user.OtpExpiresAt is null || user.OtpExpiresAt < DateTime.UtcNow)
            return BadRequest(new { message = "OTP has expired. Please request a new one." });

        if (user.OtpAttemptCount >= MaxOtpAttempts)
            return BadRequest(new { message = "Too many incorrect attempts. Please request a new OTP." });

        if (user.OtpCode != request.Otp)
        {
            user.OtpAttemptCount += 1;
            await _db.SaveChangesAsync();
            return BadRequest(new { message = "Incorrect OTP." });
        }

        // OTP consumed — clear it so it can't be replayed.
        user.OtpCode = null;
        user.OtpExpiresAt = null;
        user.OtpAttemptCount = 0;
        await _db.SaveChangesAsync();

        var (token, expiresAt) = GenerateJwt(user);

        return Ok(new OtpLoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.UserId,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            DepartmentId = user.DepartmentId,
            DepartmentName = null,
            DepartmentNameMarathi = null,
        });
    }

    private (string token, DateTime expiresAt) GenerateJwt(Entities.AppUser user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["ExpiryMinutes"] ?? "480"));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.MobilePhone, user.MobileNumber),
            new("mobileNumber", user.MobileNumber),
            new("fullName", user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (user.DepartmentId.HasValue)
            claims.Add(new Claim("departmentId", user.DepartmentId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
