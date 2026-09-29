using Microsoft.AspNetCore.Mvc;
using SMC.Master.Application.DTOs.Auth;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null) return Unauthorized(new { error = "Invalid username or password." });
        return Ok(result);
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var sent = await _authService.SendOtpAsync(request.MobileNumber, request.Email);
        return sent ? Ok() : BadRequest();
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var verified = await _authService.VerifyOtpAsync(request.MobileNumber, request.Otp);
        return verified ? Ok() : BadRequest(new { error = "Invalid or expired OTP." });
    }
}

public record SendOtpRequest(string MobileNumber, string Email);
public record VerifyOtpRequest(string MobileNumber, string Otp);
