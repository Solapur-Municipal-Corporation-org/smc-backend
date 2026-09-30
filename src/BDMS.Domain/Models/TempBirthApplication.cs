using System.ComponentModel.DataAnnotations;

namespace BDMS.Domain.Models;

/// <summary>
/// Mirrors legacy Temp_BDMS_Data — the holding table a Birth application lives in from the
/// moment OTP is verified until the document-upload step completes and it's moved into the
/// permanent table (AddInApplicationTable() in legacy). This intermediate state is not optional —
/// preserving it exactly is what makes "Upload & Submit" a distinct, later step from OTP verification.
/// </summary>
public class TempBirthApplication
{
    [Key]
    public int Id { get; set; }

    // Temp_BDMS_Id — 5-digit zero-padded sequential number, scoped to the temp table only
    public string TempApplicationNumber { get; set; } = string.Empty;

    public string FatherName { get; set; } = string.Empty;
    public string FatherAadharNumber { get; set; } = string.Empty;
    public string FatherMobileNumber { get; set; } = string.Empty;
    public string FatherEmail { get; set; } = string.Empty;
    public bool FatherAadharDocUploaded { get; set; }

    public string MotherName { get; set; } = string.Empty;
    public string MotherAadharNumber { get; set; } = string.Empty;
    public string MotherMobileNumber { get; set; } = string.Empty;
    public bool MotherAadharDocUploaded { get; set; }

    public string ChildNameEnglish { get; set; } = string.Empty;
    public string? ChildNameMarathi { get; set; }
    public DateTime ChildBirthDate { get; set; }
    public string ChildBirthPlace { get; set; } = string.Empty;
    public Gender ChildGender { get; set; }
    public string PermanentAddress { get; set; } = string.Empty;
    public string? VartaNumber { get; set; }
    public bool JanmAhawalDocUploaded { get; set; }

    public YesNoSelect IsAppliedAfterFifteenYears { get; set; }
    public YesNoSelect Navnondni { get; set; } // "was the name already informed to the Corporation before?"

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

    public decimal PaidAmount { get; set; }   // legacy sets this = amount, even though nothing was actually paid here
    public decimal AmountToPay { get; set; }
    public decimal DakhalaFee { get; set; }
    public decimal Penalty { get; set; }

    public bool OtpVerified { get; set; }

    // Set true once moved into the permanent BirthApplications table, so a temp row can't be
    // finalized twice (legacy relies on it being deleted/consumed; we keep history instead).
    public bool FinalizedToPermanent { get; set; }
}
