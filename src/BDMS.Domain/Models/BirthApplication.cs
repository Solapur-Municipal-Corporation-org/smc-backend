using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BDMS.Domain.Models;

/// <summary>
/// Mirrors legacy BDMS_Data (permanent table). A row here only exists after: form filled →
/// confirmed twice → OTP verified (created in Temp_BDMS_Data) → required documents uploaded →
/// "Upload & Submit" moved it here via AddInApplicationTable(). No payment step is part of
/// creating this record — Payment_Made_Yes_No stays "No" until handled by a separate module.
/// </summary>
public class BirthApplication
{
    [Key]
    public int Id { get; set; }

    // Per_Id — format "BCE" + fiscal-year code + 5-digit sequence, e.g. BCE262700001
    public string ApplicationNumber { get; set; } = string.Empty;

    // Links back to the temp record it was created from, for audit/traceability.
    public string TempApplicationNumber { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FatherName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{12}$", ErrorMessage = "Invalid Aadhar")]
    public string FatherAadharNumber { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid Mobile No.")]
    public string FatherMobileNumber { get; set; } = string.Empty;

    [Required, EmailAddress(ErrorMessage = "Invalid Email")]
    public string FatherEmail { get; set; } = string.Empty;

    public bool FatherAadharDocUploaded { get; set; }

    [Required, MaxLength(200)]
    public string MotherName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{12}$", ErrorMessage = "Invalid Aadhar")]
    public string MotherAadharNumber { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid Mobile No.")]
    public string MotherMobileNumber { get; set; } = string.Empty;

    public bool MotherAadharDocUploaded { get; set; }

    [Required, MaxLength(200)]
    public string ChildNameEnglish { get; set; } = string.Empty;

    public string? ChildNameMarathi { get; set; }

    [Required]
    public DateTime ChildBirthDate { get; set; }

    [Required, MaxLength(300)]
    public string ChildBirthPlace { get; set; } = string.Empty;

    [Required]
    public Gender ChildGender { get; set; }

    [Required]
    public string PermanentAddress { get; set; } = string.Empty;

    public string? VartaNumber { get; set; }
    public bool JanmAhawalDocUploaded { get; set; }

    public YesNoSelect IsAppliedAfterFifteenYears { get; set; }
    public YesNoSelect Navnondni { get; set; }

    public bool LcDocUploaded { get; set; }
    public bool SscCertDocUploaded { get; set; }
    public bool PanCardDocUploaded { get; set; }
    public bool VoterIdDocUploaded { get; set; }
    public bool AadharCardDocUploaded { get; set; }
    public bool VahanParvanaDocUploaded { get; set; }
    public bool GovtIdCardDocUploaded { get; set; }

    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string UserName { get; set; } = "Online";
    public PaymentStatus PaymentMadeYesNo { get; set; } = PaymentStatus.No;
    public string? MacAddress { get; set; }
    public ApplicationType ApplicationTypeValue { get; set; } = ApplicationType.Birth;

    public string? AckId { get; set; }
    public string AckSubject { get; set; } = string.Empty;

    public decimal PaidAmount { get; set; }
    public decimal AmountToPay { get; set; }
    public decimal DakhalaFee { get; set; }
    public decimal Penalty { get; set; }

    public ApplicationStatus VerifiedApplicationStatus { get; set; } = ApplicationStatus.Pending;

    // ---- Officer verification panel (maps BDMS_Page.aspx "CRS -Mainet" cross-check) ----
    // This is the real workflow, traced from updateResponse()/updateResponse11() in the
    // legacy code-behind — it's what actually sets VerifiedApplicationStatus, not a plain
    // Approve/Reject button.
    public RecordFoundDecision RecordFound { get; set; } = RecordFoundDecision.Pending; // Doc_Found / Doc_Uploaded
    public string? CrsMainetRegNo { get; set; }                 // CRSMainetRegNo
    public CrsMainetSource CrsMainetSourceValue { get; set; }   // SignPos
    public bool? IsNamePreviouslyReportedByCitizen { get; set; } // citizen-declared answer (read-only on scrutiny screen)
    public YesNoSelect IsNamePreviouslyReportedOperatorVerified { get; set; } = YesNoSelect.NotSelected; // operator re-answer
    public string? RegisteredChildName { get; set; }            // ChildRegName
    public DateTime? RegisteredChildBirthDate { get; set; }     // ChildRegDate
    public string? OperatorRemark { get; set; }                  // operator's freeform remark

    [NotMapped]
    public YesNoSelect ChildNameInfoOperator
    {
        get => IsNamePreviouslyReportedOperatorVerified;
        set => IsNamePreviouslyReportedOperatorVerified = value;
    }

    [NotMapped]
    public DateTime? RegisteredDate
    {
        get => RegisteredChildBirthDate;
        set => RegisteredChildBirthDate = value;
    }
    public bool DocVerified { get; set; }                        // Doc_Verified
    public DateTime? AssignedToAbhilekhapalDate { get; set; }    // Assigned_To_Abhilekhapal_Date
    public DateTime? OfficerVerifiedAt { get; set; }             // OpUploadedDate
    public bool SignedCertificateUploaded { get; set; }          // FileUpload1 -> GetDocumentSaved_InSignedPDF()
    public byte[]? IssuedCertificateContent { get; set; }        // Clerk-uploaded Janam Dakhala file
    [MaxLength(255)] public string? IssuedCertificateFileName { get; set; }
    [MaxLength(100)] public string? IssuedCertificateContentType { get; set; }

    // ---- Record lock (maps RecordLock / RecordLockTime columns + updateRecordLock()) ----
    // Legacy locks a record pessimistically the moment a clerk opens it for review, so a
    // second clerk can't edit the same application at once. Same behaviour here.
    public bool RecordLocked { get; set; }
    public string? RecordLockedByUserName { get; set; }
    public DateTime? RecordLockedAt { get; set; }

}
