using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SMC.Master.Domain.Entities;

namespace SMC.Master.Infrastructure.Authentication;

public class JwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly int _expiryMinutes;

    public JwtTokenService(string secret, string issuer, int expiryMinutes = 1440)
    {
        _secret = secret;
        _issuer = issuer;
        _expiryMinutes = expiryMinutes;
    }

    public (string token, DateTime expiresAt) GenerateToken(User user, string roleName)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_expiryMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("name", user.Username),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("departmentId", user.DepartmentId?.ToString() ?? string.Empty),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _issuer,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
