using CitizenPortal.Api.Data;
using CitizenPortal.Api.DTOs;
using CitizenPortal.Api.Department.Services;
using CitizenPortal.Api.Models;
using CitizenPortal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Security.Claims;

namespace CitizenPortal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IFileStorageService _fileStorage;
    private readonly ISmsService _sms;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(5);
    private const int MaxOtpAttempts = 5;

    public AuthController(AppDbContext db, IJwtTokenService jwtTokenService, IFileStorageService fileStorage, ISmsService sms, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _fileStorage = fileStorage;
        _sms = sms;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("register")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<CitizenResponse>> Register([FromForm] RegisterRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var existing = await FindByMobileAsync(request.MobileNumber);
        if (existing is not null)
            return Conflict(new { message = "A citizen with this mobile number already exists." });

        var citizen = new Citizen
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            MobileNumber = request.MobileNumber,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            NearestLocation = request.NearestLocation,
            City = request.City,
            State = request.State,
            Pincode = request.Pincode,
            AadhaarNumber = request.AadhaarNumber,
        };

        if (request.Document is { Length: > 0 })
        {
            var path = await _fileStorage.SaveAsync(request.Document, $"citizens/{citizen.Id}");
            citizen.DocumentPath = path;
            citizen.DocumentFileName = request.Document.FileName;
        }

        if (_db.Database.IsSqlite())
        {
            _db.Citizens.Add(citizen);
            await _db.SaveChangesAsync();
        }
        else
        {
            var connection = GetOpenConnection();
            await using var _ = connection;
            await using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO dbo.MR_DEPT_Citizens
                (FirstName, MiddleName, LastName, MobileNumber, Email, DateOfBirth, Gender,
                 AddressLine1, AddressLine2, NearestLocation, City, State, Pincode,
                 WardCode, AadhaarNumber, DocumentPath, DocumentFileName, CreatedAt, LegacyId)
                VALUES (@FirstName, @MiddleName, @LastName, @MobileNumber, @Email, @DateOfBirth, @Gender,
                        @AddressLine1, @AddressLine2, @NearestLocation, @City, @State, @Pincode,
                        NULL, @AadhaarNumber, @DocumentPath, @DocumentFileName, SYSUTCDATETIME(), @LegacyId);";

            AddParameter(command, "@FirstName", request.FirstName);
            AddParameter(command, "@MiddleName", (object?)request.MiddleName ?? DBNull.Value);
            AddParameter(command, "@LastName", request.LastName);
            AddParameter(command, "@MobileNumber", request.MobileNumber);
            AddParameter(command, "@Email", request.Email);
            AddParameter(command, "@DateOfBirth", request.DateOfBirth);
            AddParameter(command, "@Gender", request.Gender);
            AddParameter(command, "@AddressLine1", request.AddressLine1);
            AddParameter(command, "@AddressLine2", (object?)request.AddressLine2 ?? DBNull.Value);
            AddParameter(command, "@NearestLocation", (object?)request.NearestLocation ?? DBNull.Value);
            AddParameter(command, "@City", request.City);
            AddParameter(command, "@State", request.State);
            AddParameter(command, "@Pincode", request.Pincode);
            AddParameter(command, "@AadhaarNumber", request.AadhaarNumber);
            AddParameter(command, "@DocumentPath", (object?)citizen.DocumentPath ?? DBNull.Value);
            AddParameter(command, "@DocumentFileName", (object?)citizen.DocumentFileName ?? DBNull.Value);
            AddParameter(command, "@LegacyId", citizen.Id);

            await command.ExecuteNonQueryAsync();
        }

        return CreatedAtAction(nameof(Me), null, ToResponse(citizen));
    }

    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] LoginRequest request)
    {
        var citizen = await FindByMobileAsync(request.MobileNumber);
        if (citizen is null)
            return NotFound(new { message = "Citizen is not registered. Please create an account." });

        var otp = Random.Shared.Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.Add(OtpValidity);
        await UpdateCitizenOtpAsync(request.MobileNumber, otp, expiresAt, 0);
        await _sms.SendOtpAsync(request.MobileNumber, otp);

        return Ok(new
        {
            message = "OTP sent.",
            expiresInSeconds = (int)OtpValidity.TotalSeconds,
            demoOtp = _environment.IsDevelopment() ? otp : null
        });
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<AuthResponse>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var citizen = await FindByMobileAsync(request.MobileNumber);
        if (citizen is null)
            return Unauthorized(new { message = "Invalid mobile number or OTP." });

        var otpState = await GetCitizenOtpAsync(request.MobileNumber);
        if (otpState.code is null || otpState.expiresAt is null || otpState.expiresAt < DateTime.UtcNow)
            return BadRequest(new { message = "OTP has expired. Please request a new one." });
        if (otpState.attempts >= MaxOtpAttempts)
            return BadRequest(new { message = "Too many incorrect attempts. Please request a new OTP." });
        if (otpState.code != request.Otp)
        {
            await UpdateCitizenOtpAsync(request.MobileNumber, otpState.code, otpState.expiresAt, otpState.attempts + 1);
            return BadRequest(new { message = "Incorrect OTP." });
        }

        await UpdateCitizenOtpAsync(request.MobileNumber, null, null, 0);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(citizen);
        return Ok(new AuthResponse { Token = token, ExpiresAt = expiresAt, Citizen = ToResponse(citizen) });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var citizen = await FindByMobileAsync(request.MobileNumber);
        if (citizen is null)
            return NotFound(new { message = "Citizen is not registered. Please create an account." });

        var (token, expiresAt) = _jwtTokenService.GenerateToken(citizen);

        return Ok(new AuthResponse { Token = token, ExpiresAt = expiresAt, Citizen = ToResponse(citizen) });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CitizenResponse>> Me()
    {
        var mobileNumber = User.FindFirstValue("mobileNumber") ?? User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(mobileNumber)) return Unauthorized();

        var citizen = await FindByMobileAsync(mobileNumber);
        if (citizen is null) return NotFound();

        return Ok(ToResponse(citizen));
    }

    private static CitizenResponse ToResponse(Citizen c) => new()
    {
        Id = c.Id,
        FirstName = c.FirstName,
        MiddleName = c.MiddleName,
        LastName = c.LastName,
        FullName = c.FullName,
        MobileNumber = c.MobileNumber,
        Email = c.Email,
        AadhaarNumber = c.AadhaarNumber,
        AddressLine1 = c.AddressLine1,
        AddressLine2 = c.AddressLine2,
        NearestLocation = c.NearestLocation,
        Gender = c.Gender,
        City = c.City,
        State = c.State,
        Pincode = c.Pincode,
    };

    private async Task<Citizen?> FindByMobileAsync(string mobileNumber)
    {
        if (_db.Database.IsSqlite())
        {
            return await _db.Citizens
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MobileNumber == mobileNumber);
        }

        var connection = GetOpenConnection();
        await using var _ = connection;

        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT Id, FirstName, MiddleName, LastName, MobileNumber, Email,
                DateOfBirth, Gender, AddressLine1, AddressLine2, NearestLocation, City, State, Pincode, AadhaarNumber
            FROM dbo.MR_DEPT_Citizens
            WHERE MobileNumber = @mobileNumber";
        AddParameter(command, "@mobileNumber", mobileNumber);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new Citizen
        {
            Id = GuidFromInt(reader.GetInt32(0)),
            FirstName = reader.GetString(1),
            MiddleName = reader.IsDBNull(2) ? null : reader.GetString(2),
            LastName = reader.GetString(3),
            MobileNumber = reader.GetString(4),
            Email = reader.GetString(5),
            DateOfBirth = reader.GetDateTime(6),
            Gender = reader.GetString(7),
            AddressLine1 = reader.GetString(8),
            AddressLine2 = reader.IsDBNull(9) ? null : reader.GetString(9),
            NearestLocation = reader.IsDBNull(10) ? null : reader.GetString(10),
            City = reader.GetString(11),
            State = reader.GetString(12),
            Pincode = reader.GetString(13),
            AadhaarNumber = reader.GetString(14),
        };
    }

    private async Task<(string? code, DateTime? expiresAt, int attempts)> GetCitizenOtpAsync(string mobileNumber)
    {
        if (_db.Database.IsSqlite())
        {
            var citizen = await _db.Citizens.AsNoTracking().FirstOrDefaultAsync(c => c.MobileNumber == mobileNumber);
            return (citizen?.OtpCode, citizen?.OtpExpiresAt, citizen?.OtpAttemptCount ?? 0);
        }

        var connection = GetOpenConnection();
        await using var _ = connection;
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT OtpCode, OtpExpiresAt, OtpAttemptCount FROM dbo.MR_DEPT_Citizens WHERE MobileNumber = @mobileNumber";
        AddParameter(command, "@mobileNumber", mobileNumber);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return (null, null, 0);
        return (reader.IsDBNull(0) ? null : reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetDateTime(1), reader.IsDBNull(2) ? 0 : reader.GetInt32(2));
    }

    private async Task UpdateCitizenOtpAsync(string mobileNumber, string? code, DateTime? expiresAt, int attempts)
    {
        if (_db.Database.IsSqlite())
        {
            var citizen = await _db.Citizens.FirstOrDefaultAsync(c => c.MobileNumber == mobileNumber);
            if (citizen is null) return;
            citizen.OtpCode = code;
            citizen.OtpExpiresAt = expiresAt;
            citizen.OtpAttemptCount = attempts;
            await _db.SaveChangesAsync();
            return;
        }

        var connection = GetOpenConnection();
        await using var _ = connection;
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.MR_DEPT_Citizens SET OtpCode = @otp, OtpExpiresAt = @expiresAt, OtpAttemptCount = @attempts WHERE MobileNumber = @mobileNumber";
        AddParameter(command, "@otp", (object?)code ?? DBNull.Value);
        AddParameter(command, "@expiresAt", (object?)expiresAt ?? DBNull.Value);
        AddParameter(command, "@attempts", attempts);
        AddParameter(command, "@mobileNumber", mobileNumber);
        await command.ExecuteNonQueryAsync();
    }

    private DbConnection GetOpenConnection()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var connection = !string.IsNullOrWhiteSpace(connectionString)
            ? new Microsoft.Data.SqlClient.SqlConnection(connectionString)
            : _db.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        return connection;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static Guid GuidFromInt(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

}
