using BDMS.Application.DTOs;
using BDMS.Application.Services;
using BDMS.Domain.Models;
using BDMS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BDMS.API.Controllers;

[ApiController]
[Route("api/birth-applications")]
public class BirthApplicationsController : ControllerBase
{
    private readonly BdmsDbContext _db;
    private readonly IOtpService _otp;
    private readonly IBirthApplicationValidator _validator;
    private readonly IApplicationNumberService _numbers;
    private readonly IOfficerVerificationService _officerVerification;

    public BirthApplicationsController(
        BdmsDbContext db, IOtpService otp, IBirthApplicationValidator validator,
        IApplicationNumberService numbers, IOfficerVerificationService officerVerification)
    {
        _db = db;
        _otp = otp;
        _validator = validator;
        _numbers = numbers;
        _officerVerification = officerVerification;
    }

    /// <summary>
    /// Phase 1 — maps the FIRST click of "Submit" in legacy: runs full validateData() +
    /// validateDuplicateName1() + fee/penalty calc, but does NOT persist anything or send OTP.
    /// Legacy shows "Please verify filled data once again." here; we additionally return the
    /// computed fee breakdown so the citizen portal can show a real confirm screen.
    /// </summary>
    [HttpPost("precheck")]
    public async Task<ActionResult<PrecheckResultDto>> Precheck([FromBody] BirthApplicationFormDto dto)
    {
        var result = await _validator.ValidateAsync(dto);
        if (!result.IsValid)
            return Ok(new PrecheckResultDto { IsValid = false, Message = result.ErrorMessage });

        return Ok(new PrecheckResultDto
        {
            IsValid = true,
            Message = "Please verify filled data once again.",
            DakhalaFee = result.DakhalaFee,
            Penalty = result.Penalty,
            AmountToPay = result.DakhalaFee + result.Penalty,
            AckSubject = result.AckSubject
        });
    }

    /// <summary>
    /// Phase 2 — maps the SECOND click of "Submit": re-validates (legacy re-runs the same
    /// checks), generates a temp application number, and triggers OTP send to the father's
    /// mobile (falling back to mother's mobile if father's is blank — exact legacy behaviour).
    /// </summary>
    [HttpPost("temp")]
    public async Task<ActionResult<CreateTempApplicationResultDto>> CreateTemp([FromBody] BirthApplicationFormDto dto)
    {
        var result = await _validator.ValidateAsync(dto);
        if (!result.IsValid)
            return BadRequest(new { message = result.ErrorMessage });

        var tempNumber = await _numbers.GenerateTempApplicationNumberAsync();

        var mobileForOtp = !string.IsNullOrWhiteSpace(dto.FatherMobileNumber)
            ? dto.FatherMobileNumber
            : dto.MotherMobileNumber;

        var temp = new TempBirthApplication
        {
            TempApplicationNumber = tempNumber,
            FatherName = dto.FatherName,
            FatherAadharNumber = dto.FatherAadharNumber,
            FatherMobileNumber = dto.FatherMobileNumber,
            FatherEmail = dto.FatherEmail,
            MotherName = dto.MotherName,
            MotherAadharNumber = dto.MotherAadharNumber,
            MotherMobileNumber = dto.MotherMobileNumber,
            ChildNameEnglish = dto.ChildNameEnglish.ToUpperInvariant(),
            ChildNameMarathi = dto.ChildNameMarathi,
            ChildBirthDate = dto.ChildBirthDate!.Value,
            ChildBirthPlace = dto.ChildBirthPlace.ToUpperInvariant(),
            ChildGender = dto.ChildGender!.Value,
            PermanentAddress = dto.PermanentAddress,
            VartaNumber = dto.VartaNumber,
            IsAppliedAfterFifteenYears = dto.IsAppliedAfterFifteenYears!.Value,
            Navnondni = dto.Navnondni ?? YesNoSelect.NotSelected,
            EntryDate = DateTime.UtcNow,
            UserName = "Online",
            PaymentMadeYesNo = PaymentStatus.No,
            AckSubject = result.AckSubject,
            PaidAmount = result.DakhalaFee + result.Penalty,
            AmountToPay = result.DakhalaFee + result.Penalty,
            DakhalaFee = result.DakhalaFee,
            Penalty = result.Penalty,
            OtpVerified = true
        };

        _db.TempBirthApplications.Add(temp);
        await _db.SaveChangesAsync();

        var requiredDocs = GetRequiredDocuments(dto, out var atLeastTwoRequired);

        return Ok(new CreateTempApplicationResultDto
        {
            TempApplicationNumber = tempNumber,
            Message = "Application created. Upload the required documents to continue.",
            RequiredDocuments = requiredDocs,
            AtLeastTwoOfListRequired = atLeastTwoRequired
        });
    }

    private static List<RequiredDocumentDto> GetRequiredDocuments(BirthApplicationFormDto form, out bool atLeastTwoRequired)
    {
        atLeastTwoRequired = false;
        var requiredDocs = new List<RequiredDocumentDto>();
        if (form.IsAppliedAfterFifteenYears == YesNoSelect.No)
        {
            if (!string.IsNullOrWhiteSpace(form.FatherAadharNumber)) requiredDocs.Add(new RequiredDocumentDto { Key = "fatherAadhar", Label = "Father's Aadhar Card", Required = true });
            if (!string.IsNullOrWhiteSpace(form.MotherAadharNumber)) requiredDocs.Add(new RequiredDocumentDto { Key = "motherAadhar", Label = "Mother's Aadhar Card", Required = true });
        }
        if (form.Navnondni == YesNoSelect.No)
            requiredDocs.Add(new RequiredDocumentDto { Key = "janmAhawal", Label = "Birth Report (Hospital / Corporation intimation form)", Required = true });
        if (form.IsAppliedAfterFifteenYears == YesNoSelect.Yes)
        {
            atLeastTwoRequired = true;
            requiredDocs.AddRange(new[]
            {
                new RequiredDocumentDto { Key = "lc", Label = "Self-attested School Leaving Certificate" },
                new RequiredDocumentDto { Key = "sscCert", Label = "Self-attested SSC Certificate" },
                new RequiredDocumentDto { Key = "panCard", Label = "Self-attested PAN Card" },
                new RequiredDocumentDto { Key = "voterId", Label = "Self-attested Voter ID Card" },
                new RequiredDocumentDto { Key = "aadharCard", Label = "Self-attested Aadhaar ID Card" },
                new RequiredDocumentDto { Key = "vahanParvana", Label = "Self-attested Driving License" },
                new RequiredDocumentDto { Key = "govtId", Label = "Self-attested Government Organization ID Proof" }
            });
        }
        return requiredDocs;
    }

    /// <summary>
    /// Phase 3 — maps btnOk_Click: verifies OTP, and on success runs AddInTempTable() —
    /// inserts the full form into the TEMP table (not permanent yet) and returns which
    /// documents must now be uploaded, based on the after-15-years / navnondni answers.
    /// </summary>
    [HttpPost("temp/{tempApplicationNumber}/verify-otp")]
    public async Task<ActionResult<OtpVerifiedResultDto>> VerifyOtpAndCreateTempRecord(
        string tempApplicationNumber, [FromBody] VerifyOtpAndCreateTempRecordDto dto)
    {
        var existing = await _db.TempBirthApplications
            .FirstOrDefaultAsync(t => t.TempApplicationNumber == tempApplicationNumber);
        if (existing == null)
            return BadRequest(new { message = "Application record was not created yet." });

        if (existing.OtpVerified)
            return BadRequest(new { message = "This application has already been verified." });

        var mobileForOtp = !string.IsNullOrWhiteSpace(dto.Form.FatherMobileNumber)
            ? dto.Form.FatherMobileNumber
            : dto.Form.MotherMobileNumber;

        var verified = await _otp.VerifyOtpAsync(mobileForOtp, dto.OtpCode);
        if (!verified)
            return Ok(new OtpVerifiedResultDto { Verified = false, Message = "Entered OTP is wrong, Please enter correct OTP." });

        var validation = await _validator.ValidateAsync(dto.Form);
        if (!validation.IsValid)
            return BadRequest(new { message = validation.ErrorMessage });

        existing.FatherName = dto.Form.FatherName;
        existing.FatherAadharNumber = dto.Form.FatherAadharNumber;
        existing.FatherMobileNumber = dto.Form.FatherMobileNumber;
        existing.FatherEmail = dto.Form.FatherEmail;
        existing.MotherName = dto.Form.MotherName;
        existing.MotherAadharNumber = dto.Form.MotherAadharNumber;
        existing.MotherMobileNumber = dto.Form.MotherMobileNumber;
        existing.ChildNameEnglish = dto.Form.ChildNameEnglish.ToUpperInvariant();
        existing.ChildNameMarathi = dto.Form.ChildNameMarathi;
        existing.ChildBirthDate = dto.Form.ChildBirthDate!.Value;
        existing.ChildBirthPlace = dto.Form.ChildBirthPlace.ToUpperInvariant();
        existing.ChildGender = dto.Form.ChildGender!.Value;
        existing.PermanentAddress = dto.Form.PermanentAddress;
        existing.VartaNumber = dto.Form.VartaNumber;
        existing.IsAppliedAfterFifteenYears = dto.Form.IsAppliedAfterFifteenYears!.Value;
        existing.Navnondni = dto.Form.Navnondni ?? YesNoSelect.NotSelected;
        existing.EntryDate = DateTime.UtcNow;
        existing.UserName = "Online";
        existing.PaymentMadeYesNo = PaymentStatus.No;
        existing.AckSubject = validation.AckSubject;
        existing.PaidAmount = validation.DakhalaFee + validation.Penalty;
        existing.AmountToPay = validation.DakhalaFee + validation.Penalty;
        existing.DakhalaFee = validation.DakhalaFee;
        existing.Penalty = validation.Penalty;
        existing.OtpVerified = true;

        await _db.SaveChangesAsync();

        // Maps the legacy panel-visibility logic straight after AddInTempTable() succeeds.
        var requiredDocs = new List<RequiredDocumentDto>();
        var atLeastTwoRequired = false;

        if (dto.Form.IsAppliedAfterFifteenYears == YesNoSelect.No)
        {
            if (!string.IsNullOrWhiteSpace(dto.Form.FatherAadharNumber))
                requiredDocs.Add(new RequiredDocumentDto { Key = "fatherAadhar", Label = "Father's Aadhar Card", Required = true });
            if (!string.IsNullOrWhiteSpace(dto.Form.MotherAadharNumber))
                requiredDocs.Add(new RequiredDocumentDto { Key = "motherAadhar", Label = "Mother's Aadhar Card", Required = true });
        }

        if (dto.Form.Navnondni == YesNoSelect.No)
        {
            requiredDocs.Add(new RequiredDocumentDto
            {
                Key = "janmAhawal",
                Label = "Birth Report (Hospital / Corporation intimation form)",
                Required = true
            });
        }

        if (dto.Form.IsAppliedAfterFifteenYears == YesNoSelect.Yes)
        {
            atLeastTwoRequired = true;
            requiredDocs.AddRange(new[]
            {
                new RequiredDocumentDto { Key = "lc", Label = "Self-attested School Leaving Certificate", Required = false },
                new RequiredDocumentDto { Key = "sscCert", Label = "Self-attested SSC Certificate", Required = false },
                new RequiredDocumentDto { Key = "panCard", Label = "Self-attested PAN Card", Required = false },
                new RequiredDocumentDto { Key = "voterId", Label = "Self-attested Voter ID Card", Required = false },
                new RequiredDocumentDto { Key = "aadharCard", Label = "Self-attested Aadhaar ID Card", Required = false },
                new RequiredDocumentDto { Key = "vahanParvana", Label = "Self-attested Driving License", Required = false },
                new RequiredDocumentDto { Key = "govtId", Label = "Self-attested Government Organization ID Proof", Required = false },
            });
        }

        return Ok(new OtpVerifiedResultDto
        {
            Verified = true,
            RequiredDocuments = requiredDocs,
            AtLeastTwoOfListRequired = atLeastTwoRequired
        });
    }

    /// <summary>Stores the uploaded document and updates the legacy-compatible uploaded flag.</summary>
    [HttpPost("temp/{tempApplicationNumber}/documents/{documentKey}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(string tempApplicationNumber, string documentKey, IFormFile file)
    {
        var temp = await _db.TempBirthApplications
            .FirstOrDefaultAsync(t => t.TempApplicationNumber == tempApplicationNumber);
        if (temp == null) return NotFound();
        if (file is null || file.Length == 0) return BadRequest(new { message = "Please select a document." });
        if (file.Length > 10 * 1024 * 1024) return BadRequest(new { message = "Document must be 10 MB or smaller." });
        var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(new { message = "Only PDF, JPG, and PNG documents are allowed." });

        switch (documentKey)
        {
            case "fatherAadhar": temp.FatherAadharDocUploaded = true; break;
            case "motherAadhar": temp.MotherAadharDocUploaded = true; break;
            case "janmAhawal": temp.JanmAhawalDocUploaded = true; break;
            case "lc": temp.LcDocUploaded = true; break;
            case "sscCert": temp.SscCertDocUploaded = true; break;
            case "panCard": temp.PanCardDocUploaded = true; break;
            case "voterId": temp.VoterIdDocUploaded = true; break;
            case "aadharCard": temp.AadharCardDocUploaded = true; break;
            case "vahanParvana": temp.VahanParvanaDocUploaded = true; break;
            case "govtId": temp.GovtIdCardDocUploaded = true; break;
            default: return BadRequest(new { message = "Unknown document key." });
        }

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var upload = await _db.DocumentUploads
            .FirstOrDefaultAsync(x => x.TempApplicationNumber == tempApplicationNumber && x.DocumentKey == documentKey);
        if (upload is null)
        {
            upload = new DocumentUpload { TempApplicationNumber = tempApplicationNumber, DocumentKey = documentKey };
            _db.DocumentUploads.Add(upload);
        }
        upload.FileName = Path.GetFileName(file.FileName);
        upload.ContentType = file.ContentType;
        upload.Content = stream.ToArray();
        upload.UploadedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Uploaded." });
    }

    /// <summary>
    /// Phase 4 — maps the "Upload & Submit" click: validates required docs are present,
    /// then moves the record from Temp_BDMS_Data into the permanent BDMS_Data table
    /// (AddInApplicationTable()), generating the real application number. No payment
    /// happens here — matches legacy exactly.
    /// </summary>
    [HttpPost("temp/{tempApplicationNumber}/finalize")]
    public async Task<ActionResult<FinalizeResultDto>> Finalize(string tempApplicationNumber)
    {
        var temp = await _db.TempBirthApplications
            .FirstOrDefaultAsync(t => t.TempApplicationNumber == tempApplicationNumber);
        if (temp == null) return NotFound();
        if (temp.FinalizedToPermanent)
            return BadRequest(new { message = "Application already submitted." });

        // Required-document validation, exact legacy conditions.
        if (temp.IsAppliedAfterFifteenYears == YesNoSelect.No)
        {
            if (!string.IsNullOrWhiteSpace(temp.FatherAadharNumber) && !temp.FatherAadharDocUploaded)
                return Ok(new FinalizeResultDto { Success = false, Message = "Please upload father Aadhar Card." });
            if (!string.IsNullOrWhiteSpace(temp.MotherAadharNumber) && !temp.MotherAadharDocUploaded)
                return Ok(new FinalizeResultDto { Success = false, Message = "Please upload mother Aadhar Card." });
        }

        if (temp.Navnondni == YesNoSelect.No && !temp.JanmAhawalDocUploaded)
            return Ok(new FinalizeResultDto
            {
                Success = false,
                Message = "Please upload जन्म अहवाल (हॉस्पिटल / महापालिकेला जन्माची माहिती लिहून दिल्याचा फॉर्म) अपलोड करा."
            });

        if (temp.IsAppliedAfterFifteenYears == YesNoSelect.Yes)
        {
            var uploadedCount = new[]
            {
                temp.LcDocUploaded, temp.SscCertDocUploaded, temp.PanCardDocUploaded, temp.VoterIdDocUploaded,
                temp.AadharCardDocUploaded, temp.VahanParvanaDocUploaded, temp.GovtIdCardDocUploaded
            }.Count(x => x);

            if (uploadedCount < 2)
                return Ok(new FinalizeResultDto { Success = false, Message = "Please upload at least two document." });
        }

        var permanentNumber = await _numbers.GeneratePermanentApplicationNumberAsync();

        var permanent = new BirthApplication
        {
            ApplicationNumber = permanentNumber,
            TempApplicationNumber = temp.TempApplicationNumber,
            FatherName = temp.FatherName,
            FatherAadharNumber = temp.FatherAadharNumber,
            FatherMobileNumber = temp.FatherMobileNumber,
            FatherEmail = temp.FatherEmail,
            FatherAadharDocUploaded = temp.FatherAadharDocUploaded,
            MotherName = temp.MotherName,
            MotherAadharNumber = temp.MotherAadharNumber,
            MotherMobileNumber = temp.MotherMobileNumber,
            MotherAadharDocUploaded = temp.MotherAadharDocUploaded,
            ChildNameEnglish = temp.ChildNameEnglish,
            ChildNameMarathi = temp.ChildNameMarathi,
            ChildBirthDate = temp.ChildBirthDate,
            ChildBirthPlace = temp.ChildBirthPlace,
            ChildGender = temp.ChildGender,
            PermanentAddress = temp.PermanentAddress,
            VartaNumber = temp.VartaNumber,
            JanmAhawalDocUploaded = temp.JanmAhawalDocUploaded,
            IsAppliedAfterFifteenYears = temp.IsAppliedAfterFifteenYears,
            Navnondni = temp.Navnondni,
            LcDocUploaded = temp.LcDocUploaded,
            SscCertDocUploaded = temp.SscCertDocUploaded,
            PanCardDocUploaded = temp.PanCardDocUploaded,
            VoterIdDocUploaded = temp.VoterIdDocUploaded,
            AadharCardDocUploaded = temp.AadharCardDocUploaded,
            VahanParvanaDocUploaded = temp.VahanParvanaDocUploaded,
            GovtIdCardDocUploaded = temp.GovtIdCardDocUploaded,
            EntryDate = DateTime.UtcNow,
            UserName = "Online",
            PaymentMadeYesNo = PaymentStatus.No, // unchanged by this flow, exactly as legacy
            ApplicationTypeValue = ApplicationType.Birth,
            AckSubject = temp.AckSubject,
            PaidAmount = temp.PaidAmount,
            AmountToPay = temp.AmountToPay,
            DakhalaFee = temp.DakhalaFee,
            Penalty = temp.Penalty,
            VerifiedApplicationStatus = ApplicationStatus.Pending,
            IsNamePreviouslyReportedByCitizen = temp.Navnondni switch
            {
                YesNoSelect.Yes => true,
                YesNoSelect.No => false,
                _ => null
            }
        };

        _db.BirthApplications.Add(permanent);
        temp.FinalizedToPermanent = true;
        await _db.SaveChangesAsync();

        // Legacy: SendSms(...) with the application number + ack-download link, then downloadBill().
        // Dummy build: SMS is logged, not actually sent (see DummyOtpService pattern).

        return Ok(new FinalizeResultDto
        {
            Success = true,
            Message = $"Application submitted successfully. Your Application Number is - {permanentNumber}",
            ApplicationNumber = permanentNumber
        });
    }

    // ---- Officer/Clerk queue (unchanged from before — operates on the permanent table) ----

    [HttpGet("{applicationNumber}/status")]
    public async Task<ActionResult<BirthApplicationSummaryDto>> GetStatus(string applicationNumber)
    {
        var entity = await _db.BirthApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        if (entity == null) return NotFound();
        return Ok(ToSummary(entity));
    }

    /// <summary>
    /// Dummy certificate download. Approval and payment are checked here so direct API calls
    /// cannot bypass the citizen portal.
    /// </summary>
    [HttpGet("{applicationNumber}/certificate")]
    public async Task<IActionResult> DownloadCertificate(string applicationNumber)
    {
        var entity = await _db.BirthApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        if (entity == null) return NotFound(new { message = "Application not found." });

        if (entity.VerifiedApplicationStatus != ApplicationStatus.Approved ||
            entity.PaymentMadeYesNo != PaymentStatus.Yes)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Certificate download is available only after application approval and successful payment."
            });
        }

        if (entity.IssuedCertificateContent is not { Length: > 0 })
            return StatusCode(StatusCodes.Status404NotFound, new { message = "Issued certificate file is not available." });

        return File(entity.IssuedCertificateContent,
            entity.IssuedCertificateContentType ?? "application/octet-stream",
            entity.IssuedCertificateFileName ?? $"{entity.ApplicationNumber}-birth-certificate");
    }

    // Maps EditBDMS.aspx's queue list + its date-range search filters.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpGet]
    public async Task<ActionResult<List<BirthApplicationSummaryDto>>> List([FromQuery] BirthApplicationSearchDto search)
    {
        var query = _db.BirthApplications.AsQueryable();
        if (search.Status.HasValue) query = query.Where(a => a.VerifiedApplicationStatus == search.Status);
        if (search.PaymentStatus.HasValue) query = query.Where(a => a.PaymentMadeYesNo == search.PaymentStatus);
        if (search.EntryDateFrom.HasValue) query = query.Where(a => a.EntryDate.Date >= search.EntryDateFrom.Value.Date);
        if (search.EntryDateTo.HasValue) query = query.Where(a => a.EntryDate.Date <= search.EntryDateTo.Value.Date);
        if (search.ChildBirthDateFrom.HasValue) query = query.Where(a => a.ChildBirthDate.Date >= search.ChildBirthDateFrom.Value.Date);
        if (search.ChildBirthDateTo.HasValue) query = query.Where(a => a.ChildBirthDate.Date <= search.ChildBirthDateTo.Value.Date);

        var results = await query.OrderByDescending(a => a.EntryDate).Take(200).ToListAsync();
        return Ok(results.Select(ToSummary).ToList());
    }

    // Officer/Clerk opens a record for review. Opening an application always assigns the
    // review lock to the current user, clearing any stale lock left by another session.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpPost("{applicationNumber}/open-for-review")]
    public async Task<ActionResult<BirthApplicationDetailDto>> OpenForReview(string applicationNumber)
    {
        var userName = User.Identity!.Name!;
        var entity = await _db.BirthApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        if (entity == null) return NotFound();

        entity.RecordLocked = true;
        entity.RecordLockedByUserName = userName;
        entity.RecordLockedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(await ToDetailAsync(entity));
    }

    /// <summary>Stores the clerk-issued Janam Dakhala that the citizen receives after payment.</summary>
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpPost("{applicationNumber}/issued-certificate")]
    public async Task<IActionResult> UploadIssuedCertificate(string applicationNumber, IFormFile certificate)
    {
        var userName = User.Identity!.Name!;
        var entity = await _db.BirthApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        if (entity == null) return NotFound(new { message = "Application not found." });
        if (!entity.RecordLocked || entity.RecordLockedByUserName != userName)
            return Conflict(new { message = "Open this application for review first (record must be locked by you)." });
        if (certificate is null || certificate.Length == 0)
            return BadRequest(new { message = "Please select a certificate file to upload." });
        if (certificate.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "Certificate file must be 10 MB or smaller." });

        await using var stream = new MemoryStream();
        await certificate.CopyToAsync(stream);
        entity.IssuedCertificateContent = stream.ToArray();
        entity.IssuedCertificateFileName = Path.GetFileName(certificate.FileName);
        entity.IssuedCertificateContentType = string.IsNullOrWhiteSpace(certificate.ContentType)
            ? "application/octet-stream" : certificate.ContentType;
        entity.SignedCertificateUploaded = true;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Issued birth certificate uploaded.", fileName = entity.IssuedCertificateFileName });
    }

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpGet("{applicationNumber}/documents/{documentKey}")]
    public async Task<IActionResult> DownloadUploadedDocument(string applicationNumber, string documentKey)
    {
        var entity = await _db.BirthApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        if (entity == null) return NotFound();
        var upload = await _db.DocumentUploads.FirstOrDefaultAsync(x =>
            x.TempApplicationNumber == entity.TempApplicationNumber && x.DocumentKey == documentKey);
        if (upload == null) return NotFound(new { message = "Uploaded document not found." });
        return File(upload.Content, upload.ContentType, upload.FileName, enableRangeProcessing: true);
    }

    // Mirrors the legacy Cancel button / session-timeout unlock path.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpPost("{applicationNumber}/release-lock")]
    public async Task<IActionResult> ReleaseLock(string applicationNumber)
    {
        var userName = User.Identity!.Name!;
        var entity = await _db.BirthApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        if (entity == null) return NotFound();
        if (entity.RecordLocked && entity.RecordLockedByUserName != userName)
            return Conflict(new { message = "Only the clerk holding the lock can release it." });

        entity.RecordLocked = false;
        entity.RecordLockedByUserName = null;
        entity.RecordLockedAt = null;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Lock released." });
    }

    // The real officer verification screen -- exact port of BDMS_Page.aspx.cs
    // btnInsert_Click()/updateResponse()/updateResponse11(). Replaces the earlier
    // simplified Approve/Reject placeholder. See MIGRATION_SPEC.md section on
    // "Officer Verification (CRS -Mainet)" for the full trace.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpPost("{applicationNumber}/verify")]
    public async Task<ActionResult<OfficerVerificationResultDto>> Verify(
        string applicationNumber, [FromBody] OfficerVerificationDto dto)
    {
        var userName = User.Identity!.Name!;
        var result = await _officerVerification.SubmitAsync(applicationNumber, userName, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    private async Task<BirthApplicationDetailDto> ToDetailAsync(BirthApplication a)
    {
        var detail = new BirthApplicationDetailDto
        {
        ApplicationNumber = a.ApplicationNumber,
        FatherName = a.FatherName,
        FatherAadharNumber = a.FatherAadharNumber,
        FatherMobileNumber = a.FatherMobileNumber,
        FatherEmail = a.FatherEmail,
        MotherName = a.MotherName,
        MotherAadharNumber = a.MotherAadharNumber,
        MotherMobileNumber = a.MotherMobileNumber,
        ChildNameEnglish = a.ChildNameEnglish,
        ChildNameMarathi = a.ChildNameMarathi,
        ChildBirthDate = a.ChildBirthDate,
        ChildBirthPlace = a.ChildBirthPlace,
        ChildGender = a.ChildGender,
        PermanentAddress = a.PermanentAddress,
        VartaNumber = a.VartaNumber,
        Navnondni = a.Navnondni,
        IsAppliedAfterFifteenYears = a.IsAppliedAfterFifteenYears,
        AckSubject = a.AckSubject,
        UploadedDocuments = await BuildDocumentListAsync(a),
        VerifiedApplicationStatus = a.VerifiedApplicationStatus,
        RecordFound = a.RecordFound,
        CrsMainetRegNo = a.CrsMainetRegNo,
        CrsMainetSourceValue = a.CrsMainetSourceValue,
        IsNamePreviouslyReportedByCitizen = a.IsNamePreviouslyReportedByCitizen ?? (a.Navnondni switch
        {
            YesNoSelect.Yes => true,
            YesNoSelect.No => false,
            _ => null
        }),
        IsNamePreviouslyReportedOperatorVerified = a.IsNamePreviouslyReportedOperatorVerified,
        RegisteredChildBirthDate = a.RegisteredChildBirthDate,
        RegisteredChildName = a.RegisteredChildName,
        OperatorRemark = a.OperatorRemark,
        RecordLocked = a.RecordLocked,
        RecordLockedByUserName = a.RecordLockedByUserName,
        };
        return detail;
    }

    // Reconstructs the "अपलोड कागदपत्रे" document list shown live -- file names follow the
    // exact pattern observed (e.g. BCE242519727_FatherAadharCard.pdf). Built from the upload
    // flags already on the record since real file storage isn't wired up in this dummy build.
    private async Task<List<UploadedDocumentDto>> BuildDocumentListAsync(BirthApplication a)
    {
        var docs = new List<UploadedDocumentDto>();
        async Task Add(bool uploaded, string label, string key, string suffix)
        {
            if (!uploaded) return;
            var stored = await _db.DocumentUploads.FirstOrDefaultAsync(x =>
                x.TempApplicationNumber == a.TempApplicationNumber && x.DocumentKey == key);
            docs.Add(new UploadedDocumentDto
            {
                DocumentKey = key,
                Label = label,
                FileName = stored?.FileName ?? $"{a.ApplicationNumber}_{suffix}.pdf",
                DownloadUrl = stored is null
                    ? null
                    : $"/api/birth-applications/{Uri.EscapeDataString(a.ApplicationNumber)}/documents/{Uri.EscapeDataString(key)}"
            });
        }

        await Add(a.FatherAadharDocUploaded, "Father's Aadhar Card", "fatherAadhar", "FatherAadharCard");
        await Add(a.MotherAadharDocUploaded, "Mother's Aadhar Card", "motherAadhar", "MotherAadharCard");
        await Add(a.JanmAhawalDocUploaded, "Birth Report (जन्म अहवाल)", "janmAhawal", "JanmAhwal");
        await Add(a.LcDocUploaded, "School Leaving Certificate", "lc", "LC");
        await Add(a.SscCertDocUploaded, "SSC Certificate", "sscCert", "SSC");
        await Add(a.PanCardDocUploaded, "PAN Card", "panCard", "PAN");
        await Add(a.VoterIdDocUploaded, "Voter ID Card", "voterId", "VoterId");
        await Add(a.AadharCardDocUploaded, "Aadhar Card", "aadharCard", "AadharCard");
        await Add(a.VahanParvanaDocUploaded, "Driving License", "vahanParvana", "DrivingLicense");
        await Add(a.GovtIdCardDocUploaded, "Govt. ID Proof", "govtId", "GovtId");

        docs.Add(new UploadedDocumentDto { Label = "Acknowledgement", FileName = "Download Acknowledgement", IsAcknowledgement = true });
        return docs;
    }

    private static BirthApplicationSummaryDto ToSummary(BirthApplication a)
    {
        var showAmount = a.VerifiedApplicationStatus == ApplicationStatus.Approved
            && a.OfficerVerifiedAt is not null;

        return new BirthApplicationSummaryDto
        {
            Id = a.Id,
            ApplicationNumber = a.ApplicationNumber,
            CertUploadedStatus = a.RecordFound.ToString().ToUpperInvariant(),
            ChildBirthPlace = a.ChildBirthPlace,
            ChildNameEnglish = a.ChildNameEnglish,
            ChildBirthDate = a.ChildBirthDate,
            FatherName = a.FatherName,
            FatherMobileNumber = a.FatherMobileNumber,
            MotherName = a.MotherName,
            MotherMobileNumber = a.MotherMobileNumber,
            PermanentAddress = a.PermanentAddress,
            VerifiedApplicationStatus = a.VerifiedApplicationStatus,
            PaymentMadeYesNo = a.PaymentMadeYesNo,
            EntryDate = a.EntryDate,
            AckSubject = a.AckSubject,
            ShowAmount = showAmount,
            DakhalaFee = showAmount ? a.DakhalaFee : null,
            Penalty = showAmount ? a.Penalty : null,
            AmountToPay = showAmount ? a.AmountToPay : null
        };
    }
}
