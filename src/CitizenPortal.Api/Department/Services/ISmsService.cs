namespace CitizenPortal.Api.Department.Services;

/// <summary>
/// Abstraction over whatever SMS gateway SMC eventually wires up (MSG91, Twilio, a
/// government SMS gateway, etc.). No real provider is integrated anywhere in either
/// original codebase, so this ships as a console/log stub — see INTEGRATION_REPORT.md.
/// Swap <see cref="ConsoleSmsService"/> for a real implementation in Program.cs when ready.
/// </summary>
public interface ISmsService
{
    Task SendOtpAsync(string mobileNumber, string otp);
}

public class ConsoleSmsService : ISmsService
{
    private readonly ILogger<ConsoleSmsService> _logger;
    public ConsoleSmsService(ILogger<ConsoleSmsService> logger) => _logger = logger;

    public Task SendOtpAsync(string mobileNumber, string otp)
    {
        // DEV-ONLY STUB: logs the OTP instead of sending a real SMS.
        // Replace with a real gateway call before this goes anywhere near production —
        // logging OTPs is fine for local dev, not for anything else.
        _logger.LogInformation("[DEV OTP] {Mobile} -> {Otp} (expires in 5 min)", mobileNumber, otp);
        return Task.CompletedTask;
    }
}
