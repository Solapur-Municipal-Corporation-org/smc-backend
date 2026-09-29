namespace CitizenPortal.Api.Department.DTOs;

public class RequestOtpDto
{
    public string MobileNumber { get; set; } = string.Empty;
}

public class VerifyOtpDto
{
    public string MobileNumber { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class OtpLoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? DepartmentNameMarathi { get; set; }
}
