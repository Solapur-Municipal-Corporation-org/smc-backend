using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CitizenPortal.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace CitizenPortal.Api.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string token, DateTime expiresAt) GenerateToken(Citizen citizen)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "1440");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, citizen.Id.ToString()),
            new(ClaimTypes.NameIdentifier, citizen.Id.ToString()),
            new(ClaimTypes.Name, citizen.MobileNumber),
            new("fullName", citizen.FullName),
            new("mobileNumber", citizen.MobileNumber),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
