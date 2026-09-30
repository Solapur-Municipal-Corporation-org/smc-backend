using BDMS.Application.DTOs;
using BDMS.Domain.Models;

namespace BDMS.Application.Services;

public sealed class BirthValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DakhalaFee { get; set; }
    public decimal Penalty { get; set; }
    public string AckSubject { get; set; } = string.Empty;

    public static BirthValidationResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };
}

public interface IBirthApplicationValidator
{
    Task<BirthValidationResult> ValidateAsync(BirthApplicationFormDto dto);
}

public interface IApplicationNumberService
{
    Task<string> GenerateTempApplicationNumberAsync();
    Task<string> GeneratePermanentApplicationNumberAsync();
    Task<string> GenerateDeathTempApplicationNumberAsync();
    Task<string> GenerateDeathPermanentApplicationNumberAsync();
}

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(string userName, string password);
}

public interface IOfficerVerificationService
{
    Task<OfficerVerificationResultDto> SubmitAsync(string applicationNumber, string officerUserName, OfficerVerificationDto dto);
}

public interface IOtpService
{
    Task<string> SendOtpAsync(string applicationNumber, string mobileNumber, string? email);
    Task<bool> VerifyOtpAsync(string mobileNumber, string code);
}

public interface IPasswordCipher
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public interface IPaymentService
{
    Task<PaymentRecord> InitiateAsync(string applicationNumber, decimal amount);
    Task<PaymentRecord> ConfirmAsync(string transactionId);
}
