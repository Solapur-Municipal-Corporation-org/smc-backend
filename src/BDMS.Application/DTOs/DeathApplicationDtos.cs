using BDMS.Domain.Models;
namespace BDMS.Application.DTOs;

public class DeathApplicationFormDto
{
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantAadharNumber { get; set; } = string.Empty;
    public string ApplicantMobileNumber { get; set; } = string.Empty;
    public string PermanentAddress { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DeadPersonName { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public DateTime? DeathDate { get; set; }
    public string DeathPlace { get; set; } = string.Empty;
    public string DeadPersonAadharNumber { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string FatherHusbandName { get; set; } = string.Empty;
}
public class DeathOtpVerificationDto { public DeathApplicationFormDto Form { get; set; } = new(); public string OtpCode { get; set; } = string.Empty; }
public class DeathApplicationSummaryDto
{
    public int Id { get; set; } public string ApplicationNumber { get; set; } = string.Empty;
    public string DeadPersonName { get; set; } = string.Empty; public DateTime DeathDate { get; set; }
    public string DeathPlace { get; set; } = string.Empty; public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantMobileNumber { get; set; } = string.Empty; public string PermanentAddress { get; set; } = string.Empty;
    public ApplicationStatus VerifiedApplicationStatus { get; set; } public PaymentStatus PaymentMadeYesNo { get; set; }
    public DateTime EntryDate { get; set; } public string CertUploadedStatus { get; set; } = string.Empty;
}
public class DeathApplicationDetailDto : DeathApplicationSummaryDto
{
    public string ApplicantAadharNumber { get; set; } = string.Empty; public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; } public string DeadPersonAadharNumber { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty; public string FatherHusbandName { get; set; } = string.Empty;
    public string? CrsMainetRegNo { get; set; } public CrsMainetSource CrsMainetSourceValue { get; set; }
    public string? OperatorRemark { get; set; } public bool RecordLocked { get; set; }
    public DateTime? RegisteredDeathDate { get; set; }
    public List<UploadedDocumentDto> UploadedDocuments { get; set; } = new();
}
