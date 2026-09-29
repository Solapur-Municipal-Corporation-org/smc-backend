using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.DTOs;

/// <summary>
/// Sent as multipart/form-data (not JSON) because it includes an optional document file.
/// </summary>
public class RegisterRequest
{
    // ---- Basic info ----
    [Required, MinLength(2)]
    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    [Required, MinLength(1)]
    public string LastName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required, RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Select a valid gender")]
    public string Gender { get; set; } = string.Empty;

    // ---- Address ----
    [Required, MinLength(5)]
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string? NearestLocation { get; set; }

    [Required, MinLength(2)]
    public string City { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string State { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Enter a valid 6-digit pincode")]
    public string Pincode { get; set; } = string.Empty;

    // ---- Documents ----
    [Required, RegularExpression(@"^\d{12}$", ErrorMessage = "Enter a valid 12-digit Aadhaar number")]
    public string AadhaarNumber { get; set; } = string.Empty;

    public IFormFile? Document { get; set; }
}

/// <summary>Citizen login begins with the mobile number stored in MR_DEPT_Citizens.</summary>
public class LoginRequest
{
    [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
    public string MobileNumber { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    [Required, RegularExpression(@"^[6-9]\d{9}$")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$")]
    public string Otp { get; set; } = string.Empty;
}

public class CitizenResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AadhaarNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? NearestLocation { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public CitizenResponse Citizen { get; set; } = new();
}
