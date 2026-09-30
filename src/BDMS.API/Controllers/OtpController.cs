using BDMS.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.API.Controllers;

public record SendOtpRequest(string ApplicationNumber, string MobileNumber, string? Email);
public record VerifyOtpRequest(string MobileNumber, string Code);

[ApiController]
[Route("api/otp")]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otp;

    public OtpController(IOtpService otp) => _otp = otp;

    // Maps legacy sendOTP()
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendOtpRequest req)
    {
        await _otp.SendOtpAsync(req.ApplicationNumber, req.MobileNumber, req.Email);
        return Ok(new { message = $"OTP is sent to your mobile - {req.MobileNumber}. Please enter OTP to proceed." });
    }

    // Maps legacy verifyOTP()
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyOtpRequest req)
    {
        var ok = await _otp.VerifyOtpAsync(req.MobileNumber, req.Code);
        if (!ok) return BadRequest(new { message = "Entered OTP is wrong, Please enter correct OTP." });
        return Ok(new { message = "OTP verified." });
    }
}
