using SMC.Master.Application.DTOs.Auth;

namespace SMC.Master.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<bool> SendOtpAsync(string mobileNumber, string email);
    Task<bool> VerifyOtpAsync(string mobileNumber, string otp);
}
