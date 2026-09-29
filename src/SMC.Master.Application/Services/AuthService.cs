using SMC.Master.Application.DTOs.Auth;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.Application.Services;

public class AuthService : IAuthService
{
    // Repositories / JwtTokenService are injected via SMC.Master.Infrastructure in Program.cs.
    // This is the orchestration layer only — password verification and token issuance
    // live in SMC.Master.Infrastructure.Authentication.

    public Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        throw new NotImplementedException("Wire up UserRepository + JwtTokenService here.");
    }

    public Task<bool> SendOtpAsync(string mobileNumber, string email)
    {
        // Single shared OTP sent once both mobile + email are provided (simulated, no real gateway).
        throw new NotImplementedException();
    }

    public Task<bool> VerifyOtpAsync(string mobileNumber, string otp)
    {
        throw new NotImplementedException();
    }
}
