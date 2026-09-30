using BDMS.Domain.Models;

namespace BDMS.Application.DTOs;

/// <summary>The full single-page birth registration form.</summary>
public class BirthApplicationFormDto
{
    public string FatherName { get; set; } = string.Empty;
    public string FatherAadharNumber { get; set; } = string.Empty;
    public string FatherMobileNumber { get; set; } = string.Empty;
    public string FatherEmail { get; set; } = string.Empty;

    public string MotherName { get; set; } = string.Empty;
    public string MotherAadharNumber { get; set; } = string.Empty;
    public string MotherMobileNumber { get; set; } = string.Empty;

    public string PermanentAddress { get; set; } = string.Empty;

    public DateTime? ChildBirthDate { get; set; }
    public Gender? ChildGender { get; set; }
    public string ChildBirthPlace { get; set; } = string.Empty;
    public string ChildNameMarathi { get; set; } = string.Empty;
    public string ChildNameEnglish { get; set; } = string.Empty;

    public YesNoSelect? IsAppliedAfterFifteenYears { get; set; }
    public YesNoSelect? Navnondni { get; set; }
    public string? VartaNumber { get; set; }
}

public class PrecheckResultDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DakhalaFee { get; set; }
    public decimal Penalty { get; set; }
    public decimal AmountToPay { get; set; }
    public string? AckSubject { get; set; }
}

public class CreateTempApplicationResultDto
{
    public string TempApplicationNumber { get; set; } = string.Empty;
    public string OtpSentToMobile { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<RequiredDocumentDto> RequiredDocuments { get; set; } = new();
    public bool AtLeastTwoOfListRequired { get; set; }
}

public class VerifyOtpAndCreateTempRecordDto
{
    public BirthApplicationFormDto Form { get; set; } = new();
    public string OtpCode { get; set; } = string.Empty;
}

public class RequiredDocumentDto
{
    public string Key { get; set; } = string.Empty;      // e.g. "fatherAadhar"
    public string Label { get; set; } = string.Empty;
    public bool Required { get; set; }
}

public class OtpVerifiedResultDto
{
    public bool Verified { get; set; }
    public string? Message { get; set; }
    public List<RequiredDocumentDto> RequiredDocuments { get; set; } = new();
    public bool AtLeastTwoOfListRequired { get; set; }
}

public class FinalizeResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ApplicationNumber { get; set; }
}

public class BirthApplicationSummaryDto
{
    public int Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string CertUploadedStatus { get; set; } = string.Empty;  // maps "Cert Uploaded Status" column -- RecordFound as legacy displays it
    public string ChildBirthPlace { get; set; } = string.Empty;     // maps "Balache Birth Place" column
    public string ChildNameEnglish { get; set; } = string.Empty;
    public DateTime ChildBirthDate { get; set; }
    public string FatherName { get; set; } = string.Empty;
    public string FatherMobileNumber { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string MotherMobileNumber { get; set; } = string.Empty;
    public string PermanentAddress { get; set; } = string.Empty;
    public ApplicationStatus VerifiedApplicationStatus { get; set; }
    public PaymentStatus PaymentMadeYesNo { get; set; }
    public DateTime EntryDate { get; set; }
    public string AckSubject { get; set; } = string.Empty;
    public bool ShowAmount { get; set; }
    public decimal? DakhalaFee { get; set; }
    public decimal? Penalty { get; set; }
    public decimal? AmountToPay { get; set; }
}

/// <summary>Maps the search filters on EditBDMS.aspx's queue list (date-range pickers above the table).</summary>
public class BirthApplicationSearchDto
{
    public ApplicationStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? EntryDateFrom { get; set; }
    public DateTime? EntryDateTo { get; set; }
    public DateTime? ChildBirthDateFrom { get; set; }
    public DateTime? ChildBirthDateTo { get; set; }
}

public class ReviewDecisionDto
{
    public string Decision { get; set; } = string.Empty; // deprecated -- kept only so old callers get a clear error; use OfficerVerificationDto via /verify instead
}

/// <summary>Maps the full "CRS -Mainet" verification panel submitted from BDMS_Page.aspx's
/// btnInsert_Click. Field names mirror the legacy controls directly.</summary>
public class OfficerVerificationDto
{
    public RecordFoundDecision RecordFound { get; set; }          // ddlRecordFound
    public string? CrsMainetRegNo { get; set; }                    // txtCRSMainetRegNo
    public CrsMainetSource CrsMainetSourceValue { get; set; }      // ddlCRSMainet
    public YesNoSelect IsNamePreviouslyReportedOperatorVerified { get; set; } // ddlChildNameInfo
    public DateTime? RegisteredChildBirthDate { get; set; }        // txtRegisteredDate
    public string? RegisteredChildName { get; set; }               // txtRegsiteredChildName
    public string? Remark { get; set; }                            // txtRemark
    public bool SignedCertificateUploaded { get; set; }            // FileUpload1.HasFile (dummy: a checkbox in the demo UI)
}

public class OfficerVerificationResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ApplicationStatus? NewStatus { get; set; }
}

/// <summary>Full application detail for the officer review screen — every field the citizen
/// entered (read-only, for context), matching the layout of the actual public form
/// (जन्म दाखला मिळण्याबाबत नमुना अर्ज), plus whatever verification data already exists.
/// Deliberately does NOT include payment/fee amounts -- the real legacy verification screen
/// doesn't show them either; payment only ever surfaces after approval, in a separate
/// not-yet-built module. Showing it here earlier was a mistake in an earlier version of
/// this build and has been removed.</summary>
public class BirthApplicationDetailDto
{
    public string ApplicationNumber { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string FatherAadharNumber { get; set; } = string.Empty;
    public string FatherMobileNumber { get; set; } = string.Empty;
    public string FatherEmail { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string MotherAadharNumber { get; set; } = string.Empty;
    public string MotherMobileNumber { get; set; } = string.Empty;
    public string ChildNameEnglish { get; set; } = string.Empty;
    public string? ChildNameMarathi { get; set; }
    public DateTime ChildBirthDate { get; set; }
    public string ChildBirthPlace { get; set; } = string.Empty;
    public Gender ChildGender { get; set; }
    public string PermanentAddress { get; set; } = string.Empty;
    public string? VartaNumber { get; set; }
    public YesNoSelect Navnondni { get; set; }
    public YesNoSelect IsAppliedAfterFifteenYears { get; set; }
    public string AckSubject { get; set; } = string.Empty;

    public List<UploadedDocumentDto> UploadedDocuments { get; set; } = new();

    public ApplicationStatus VerifiedApplicationStatus { get; set; }
    public RecordFoundDecision RecordFound { get; set; }
    public string? CrsMainetRegNo { get; set; }
    public CrsMainetSource CrsMainetSourceValue { get; set; }
    public bool? IsNamePreviouslyReportedByCitizen { get; set; }
    public YesNoSelect IsNamePreviouslyReportedOperatorVerified { get; set; }
    public DateTime? RegisteredChildBirthDate { get; set; }
    public string? RegisteredChildName { get; set; }
    public string? OperatorRemark { get; set; }

    public bool RecordLocked { get; set; }
    public string? RecordLockedByUserName { get; set; }
}

/// <summary>One row in the "अपलोड कागदपत्रे" / uploaded-documents table shown above the
/// verification panel. File names follow the exact naming pattern seen live
/// (e.g. BCE242519727_FatherAadharCard.pdf) -- reconstructed from the upload flags already
/// on the record, since real file storage isn't wired up in this dummy build yet.</summary>
public class UploadedDocumentDto
{
    public string DocumentKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsAcknowledgement { get; set; }
    public string? DownloadUrl { get; set; }
}
