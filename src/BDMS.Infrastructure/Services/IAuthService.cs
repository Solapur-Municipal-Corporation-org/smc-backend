using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using BDMS.Application.DTOs;
using BDMS.Application.Services;
using BDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BDMS.Infrastructure.Services;

/// <summary>
/// Mirrors legacy AuthenticateUser() (encrypt password with the legacy cipher, look up
/// Login_Details / spAuthenticateUser, fetch role via Proc_LoginRoleDetails) and the
/// role-based redirect in btnLogin_Click. Legacy used a 30-minute FormsAuthenticationTicket;
/// this issues a JWT with the same 30-minute expiry instead, since the new API is stateless.
/// </summary>
public class AuthService : IAuthService
{
    private readonly BdmsDbContext _db;
    private readonly IPasswordCipher _cipher;
    private readonly IConfiguration _config;

    public AuthService(BdmsDbContext db, IPasswordCipher cipher, IConfiguration config)
    {
        _db = db;
        _cipher = cipher;
        _config = config;
    }

    public async Task<LoginResponseDto> LoginAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return new LoginResponseDto { Success = false, Message = "Please enter User name" };
        if (string.IsNullOrWhiteSpace(password))
            return new LoginResponseDto { Success = false, Message = "Please enter passowrd" };

        var encrypted = _cipher.Encrypt(password);

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == userName.Trim() && u.PasswordEncrypted == encrypted && u.IsActive);

        if (user == null)
            return new LoginResponseDto { Success = false, Message = "Invalid user name and/or password." };

        if (user.UserRole != "Operator")
            return new LoginResponseDto { Success = false, Message = "This account role is no longer authorized for the BDMS portal." };

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var expires = DateTime.UtcNow.AddMinutes(30); // matches legacy FormsAuthenticationTicket expiry
        var token = BuildJwt(user.UserName, user.UserRole, user.Dept, expires);

        return new LoginResponseDto
        {
            Success = true,
            Token = token,
            UserName = user.UserName,
            Role = user.UserRole,
            Dept = user.Dept,
            RedirectTo = RoleRedirect(user.UserRole), // mirrors legacy's role.Equals("Admin")/"Operator"/"Dash"/"Test" branches
            ExpiresAt = expires
        };
    }

    // Only Clerk accounts are permitted to use the BDMS portal.
    private static string RoleRedirect(string role) => role switch
    {
        "Operator" => "/clerk",
        _ => "/"
    };

    private string BuildJwt(string userName, string role, string? dept, DateTime expires)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Role, role),
        };
        if (!string.IsNullOrEmpty(dept)) claims.Add(new Claim("dept", dept));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
