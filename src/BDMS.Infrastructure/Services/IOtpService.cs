using BDMS.Application.Services;
using BDMS.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BDMS.Infrastructure.Services;

/// <summary>
/// Dummy implementation for migration: does NOT call a real SMS/email gateway.
/// Behaviour mirrors legacy GenerateOTP()/sendOTP()/verifyOTP() (insert row, verify against
/// latest row for the mobile number) but the "send" step just logs, and a fixed code from
/// config is always accepted in addition to the generated one — matching your "dummy OTP" requirement.
/// Swap this class out for a real gateway later; the interface contract stays the same.
/// </summary>
public class DummyOtpService : IOtpService
{
    private readonly IConfiguration _config;
    private readonly ILogger<DummyOtpService> _logger;

    public DummyOtpService(IConfiguration config, ILogger<DummyOtpService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task<string> SendOtpAsync(string applicationNumber, string mobileNumber, string? email)
    {
        var code = _config["DummyServices:FixedOtpCode"] ?? "123456";

        // Legacy sent real SMS + email here. Dummy mode: just log what *would* have been sent.
        _logger.LogInformation(
            "[DUMMY OTP] Would send OTP {Code} to mobile {Mobile} / email {Email}",
            code, mobileNumber, email);

        return Task.FromResult(code);
    }

    public Task<bool> VerifyOtpAsync(string mobileNumber, string code)
    {
        return Task.FromResult(code == (_config["DummyServices:FixedOtpCode"] ?? "123456"));
    }
}
