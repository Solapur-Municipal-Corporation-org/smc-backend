using System.ComponentModel.DataAnnotations;

namespace BDMS.Domain.Models;

/// <summary>Permanent death-certificate application. Kept separate from birth data while sharing the BDMS database and workflow.</summary>
public class DeathApplication
{
    [Key] public int Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string TempApplicationNumber { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string ApplicantName { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\d{12}$")] public string ApplicantAadharNumber { get; set; } = string.Empty;
    [Required, RegularExpression(@"^[0-9]{10}$")] public string ApplicantMobileNumber { get; set; } = string.Empty;
    [Required] public string PermanentAddress { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string DeadPersonName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DeathDate { get; set; }
    [Required, MaxLength(300)] public string DeathPlace { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\d{12}$")] public string DeadPersonAadharNumber { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string MotherName { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string FatherHusbandName { get; set; } = string.Empty;
    public bool ApplicantAadharDocUploaded { get; set; }
    public bool DeathProofDocUploaded { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string UserName { get; set; } = "Online";
    public PaymentStatus PaymentMadeYesNo { get; set; } = PaymentStatus.No;
    public ApplicationType ApplicationTypeValue { get; set; } = ApplicationType.Death;
    public string AckSubject { get; set; } = "Death Certificate";
    public ApplicationStatus VerifiedApplicationStatus { get; set; } = ApplicationStatus.Pending;
    public RecordFoundDecision RecordFound { get; set; } = RecordFoundDecision.Pending;
    public string? CrsMainetRegNo { get; set; }
    public CrsMainetSource CrsMainetSourceValue { get; set; }
    public string? OperatorRemark { get; set; }
    public DateTime? RegisteredDeathDate { get; set; }
    public bool SignedCertificateUploaded { get; set; }

    public DateTime? OfficerVerifiedAt { get; set; }
    public bool RecordLocked { get; set; }
    public string? RecordLockedByUserName { get; set; }
    public DateTime? RecordLockedAt { get; set; }
}

public class TempDeathApplication
{
    [Key] public int Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string TempApplicationNumber { get; set; } = string.Empty;
    public string TempApplicationNumberValue { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantAadharNumber { get; set; } = string.Empty;
    public string ApplicantMobileNumber { get; set; } = string.Empty;
    public string PermanentAddress { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DeadPersonName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DeathDate { get; set; }
    public string DeathPlace { get; set; } = string.Empty;
    public string DeadPersonAadharNumber { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string FatherHusbandName { get; set; } = string.Empty;
    public bool ApplicantAadharDocUploaded { get; set; }
    public bool DeathProofDocUploaded { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string UserName { get; set; } = "Online";
    public PaymentStatus PaymentMadeYesNo { get; set; } = PaymentStatus.No;
    public ApplicationType ApplicationTypeValue { get; set; } = ApplicationType.Death;
    public string AckSubject { get; set; } = "Death Certificate";
    public ApplicationStatus VerifiedApplicationStatus { get; set; } = ApplicationStatus.Pending;
    public RecordFoundDecision RecordFound { get; set; } = RecordFoundDecision.Pending;
    public string? CrsMainetRegNo { get; set; }
    public CrsMainetSource CrsMainetSourceValue { get; set; }
    public string? OperatorRemark { get; set; }
    public DateTime? RegisteredDeathDate { get; set; }
    public bool SignedCertificateUploaded { get; set; }
    public bool OtpVerified { get; set; }
    public DateTime? OfficerVerifiedAt { get; set; }
    public bool RecordLocked { get; set; }
    public string? RecordLockedByUserName { get; set; }
    public DateTime? RecordLockedAt { get; set; }
    public bool FinalizedToPermanent { get; set; }
}
