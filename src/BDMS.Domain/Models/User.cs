using System.ComponentModel.DataAnnotations;

namespace BDMS.Domain.Models;

/// <summary>
/// Mirrors legacy Login_Details table (used by Login_Module/LoginPageNew.aspx across the
/// corporation's services generally, not just BDMS). Only the columns relevant to Officer/
/// Clerk login are modeled here — DOB day/month/year fields from the legacy registration
/// form aren't needed for this migration and are left out.
/// </summary>
public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }

    [Required, MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    // Stored using the exact legacy reversible AES scheme (see LegacyPasswordCipher) so this
    // migration is byte-for-byte compatible with existing Login_Details rows if ever imported.
    // NOTE: this is a known weakness in the legacy system (reversible encryption, not a salted
    // hash) — flagged here rather than silently "fixed", per your instruction to preserve logic.
    // Recommend moving to ASP.NET Core Identity + bcrypt/PBKDF2 hashing in a later hardening pass.
    [Required]
    public string PasswordEncrypted { get; set; } = string.Empty;

    // Legacy User_Role values: "Admin" (Officer), "Operator" (Clerk), "Dash", "Test".
    [Required, MaxLength(50)]
    public string UserRole { get; set; } = string.Empty;

    public string? Dept { get; set; }
    public string? MobileNumber { get; set; }
    public string? EmailId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}
