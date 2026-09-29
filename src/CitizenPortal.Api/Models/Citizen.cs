using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CitizenPortal.Api.Models;

public class Citizen
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // ---- Basic info ----
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [Required, MaxLength(10)]
    public string Gender { get; set; } = string.Empty;

    /// <summary>Convenience combined name — not a DB column. Existing code (certificates, JWT claims) reads this.</summary>
    [NotMapped]
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}".Trim()
        : $"{FirstName} {MiddleName} {LastName}".Trim();

    // ---- Address ----
    [Required, MaxLength(200)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    [MaxLength(150)]
    public string? NearestLocation { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required, MaxLength(6)]
    public string Pincode { get; set; } = string.Empty;

    // ---- Documents ----
    [Required, MaxLength(12)]
    public string AadhaarNumber { get; set; } = string.Empty;

    /// <summary>Relative path (under Uploads/) to the citizen's uploaded ID document, if any.</summary>
    [MaxLength(500)]
    public string? DocumentPath { get; set; }

    [MaxLength(200)]
    public string? DocumentFileName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public int OtpAttemptCount { get; set; }

    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
