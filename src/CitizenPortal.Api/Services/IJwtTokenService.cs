using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Services;

public interface IJwtTokenService
{
    (string token, DateTime expiresAt) GenerateToken(Citizen citizen);
}
