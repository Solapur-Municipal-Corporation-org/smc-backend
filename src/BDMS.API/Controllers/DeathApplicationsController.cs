using System.Text.RegularExpressions;
using BDMS.Application.DTOs;
using BDMS.Application.Services;
using BDMS.Domain.Models;
using BDMS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BDMS.API.Controllers;

[ApiController]
[Route("api/death-applications")]
public class DeathApplicationsController : ControllerBase
{
    private readonly BdmsDbContext _db;
    private readonly IApplicationNumberService _numbers;
    private readonly IOtpService _otp;
    private static readonly Regex NameInvalid = new(@"[0-9@#$!%^&*]");
    private static readonly Regex Aadhar = new(@"^\d{12}$");
    private static readonly Regex Mobile = new(@"^\d{10}$");

    public DeathApplicationsController(BdmsDbContext db, IApplicationNumberService numbers, IOtpService otp)
        => (_db, _numbers, _otp) = (db, numbers, otp);

    [HttpPost("precheck")]
    public async Task<ActionResult<PrecheckResultDto>> Precheck(DeathApplicationFormDto form)
    {
        var error = await Validate(form);
        return Ok(new PrecheckResultDto { IsValid = error is null, Message = error, AckSubject = "Death Certificate" });
    }

    [HttpPost("temp")]
    public async Task<ActionResult<CreateTempApplicationResultDto>> CreateTemp(DeathApplicationFormDto form)
    {
        var error = await Validate(form);
        if (error is not null) return BadRequest(new { message = error });
        var number = await _numbers.GenerateDeathTempApplicationNumberAsync();
        var entity = Map(form, new TempDeathApplication
        {
            TempApplicationNumberValue = number,
            TempApplicationNumber = number,
            ApplicationNumber = number,
            OtpVerified = true
        });
        _db.TempDeathApplications.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(new CreateTempApplicationResultDto { TempApplicationNumber = number, Message = "Application created. Upload the required documents to continue.", RequiredDocuments = Documents() });
    }

    [HttpPost("temp/{number}/verify-otp")]
    public async Task<ActionResult<OtpVerifiedResultDto>> VerifyOtp(string number, DeathOtpVerificationDto input)
    {
        var error = await Validate(input.Form);
        if (error is not null) return BadRequest(new { message = error });
        if (input.OtpCode != "123456") return Ok(new OtpVerifiedResultDto { Verified = false, Message = "Invalid OTP." });
        var entity = await _db.TempDeathApplications.FirstOrDefaultAsync(x => x.TempApplicationNumberValue == number);
        if (entity is null)
        {
            entity = Map(input.Form, new TempDeathApplication { TempApplicationNumberValue = number, TempApplicationNumber = number, ApplicationNumber = number, OtpVerified = true });
            _db.TempDeathApplications.Add(entity);
        }
        await _db.SaveChangesAsync();
        return Ok(new OtpVerifiedResultDto { Verified = true, Message = "OTP verified.", RequiredDocuments = Documents(), AtLeastTwoOfListRequired = false });
    }

    [HttpPost("temp/{number}/documents/{key}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(string number, string key, IFormFile file)
    {
        var app = await _db.TempDeathApplications.FirstOrDefaultAsync(x => x.TempApplicationNumberValue == number);
        if (app is null || !app.OtpVerified) return NotFound(new { message = "Verified temporary application not found." });
        if (file is null || file.Length == 0) return BadRequest(new { message = "Please select a document." });
        if (file.Length > 10 * 1024 * 1024) return BadRequest(new { message = "Document must be 10 MB or smaller." });
        var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant())) return BadRequest(new { message = "Only PDF, JPG, and PNG documents are allowed." });
        if (key == "applicantAadhar") app.ApplicantAadharDocUploaded = true;
        else if (key == "deathProof") app.DeathProofDocUploaded = true;
        else return BadRequest(new { message = "Unknown document." });
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var upload = await _db.DocumentUploads.FirstOrDefaultAsync(x => x.TempApplicationNumber == number && x.DocumentKey == key);
        if (upload is null)
        {
            upload = new DocumentUpload { TempApplicationNumber = number, DocumentKey = key };
            _db.DocumentUploads.Add(upload);
        }
        upload.FileName = Path.GetFileName(file.FileName);
        upload.ContentType = file.ContentType;
        upload.Content = stream.ToArray();
        upload.UploadedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(); return Ok(new { message = "Document uploaded." });
    }

    [HttpPost("temp/{number}/finalize")]
    public async Task<ActionResult<FinalizeResultDto>> Finalize(string number)
    {
        var temp = await _db.TempDeathApplications.FirstOrDefaultAsync(x => x.TempApplicationNumberValue == number);
        if (temp is null || !temp.OtpVerified) return BadRequest(new FinalizeResultDto { Success = false, Message = "OTP verification is required." });
        if (temp.FinalizedToPermanent) return BadRequest(new FinalizeResultDto { Success = false, Message = "Application has already been submitted." });
        if (!temp.ApplicantAadharDocUploaded || !temp.DeathProofDocUploaded) return BadRequest(new FinalizeResultDto { Success = false, Message = "Upload all required documents first." });
        var permanentNumber = await _numbers.GenerateDeathPermanentApplicationNumberAsync();
        var app = new DeathApplication(); Copy(temp, app); app.ApplicationNumber = permanentNumber; app.TempApplicationNumber = number; app.EntryDate = DateTime.UtcNow;
        _db.DeathApplications.Add(app);
        temp.FinalizedToPermanent = true;
        var affectedRows = await _db.SaveChangesAsync();
        var persisted = affectedRows > 0 && await _db.DeathApplications
            .AnyAsync(a => a.ApplicationNumber == permanentNumber);
        if (!persisted)
            return StatusCode(StatusCodes.Status500InternalServerError, new FinalizeResultDto
            {
                Success = false,
                Message = "The death certificate application could not be saved."
            });
        return Ok(new FinalizeResultDto { Success = true, Message = $"Application submitted successfully. Your Application Number is - {permanentNumber}", ApplicationNumber = permanentNumber });
    }

    [HttpGet("{number}/status")]
    public async Task<ActionResult<DeathApplicationSummaryDto>> Status(string number)
    { var app = await _db.DeathApplications.FirstOrDefaultAsync(x => x.ApplicationNumber == number); return app is null ? NotFound() : Ok(Summary(app)); }

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpGet]
    public async Task<ActionResult<List<DeathApplicationSummaryDto>>> List()
        => Ok((await _db.DeathApplications.OrderByDescending(x => x.EntryDate).Take(200).ToListAsync()).Select(Summary).ToList());

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpPost("{number}/open-for-review")]
    public async Task<ActionResult<DeathApplicationDetailDto>> Open(string number)
    {
        var app = await _db.DeathApplications.FirstOrDefaultAsync(x => x.ApplicationNumber == number); if (app is null) return NotFound();
        app.RecordLocked = true; app.RecordLockedByUserName = User.Identity!.Name; app.RecordLockedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return Ok(Detail(app));
    }

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpPost("{number}/release-lock")]
    public async Task<IActionResult> Release(string number)
    { var app = await _db.DeathApplications.FirstOrDefaultAsync(x => x.ApplicationNumber == number); if (app is null) return NotFound(); if (app.RecordLockedByUserName != User.Identity!.Name) return Conflict(new { message = "Only the clerk holding the lock can release it." }); app.RecordLocked = false; app.RecordLockedByUserName = null; await _db.SaveChangesAsync(); return Ok(); }

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Operator")]
    [HttpPost("{number}/verify")]
    public async Task<ActionResult<OfficerVerificationResultDto>> Verify(string number, OfficerVerificationDto dto)
    {
        var app = await _db.DeathApplications.FirstOrDefaultAsync(x => x.ApplicationNumber == number); var user = User.Identity!.Name!;
        if (app is null) return NotFound(); if (!app.RecordLocked || app.RecordLockedByUserName != user) return BadRequest(new OfficerVerificationResultDto { Success = false, Message = "Open this application for review first." });
        if (dto.RecordFound == RecordFoundDecision.Pending) return BadRequest(new OfficerVerificationResultDto { Success = false, Message = "Please select record found or not." });
        if (dto.RecordFound == RecordFoundDecision.Yes && (string.IsNullOrWhiteSpace(dto.CrsMainetRegNo) || dto.CrsMainetSourceValue == CrsMainetSource.NotSelected || dto.RegisteredChildBirthDate is null || !dto.SignedCertificateUploaded)) return BadRequest(new OfficerVerificationResultDto { Success = false, Message = "Registration number, source, registered death date, and signed certificate are required." });
        app.RecordFound = dto.RecordFound; app.CrsMainetRegNo = dto.CrsMainetRegNo; app.CrsMainetSourceValue = dto.CrsMainetSourceValue; app.RegisteredDeathDate = dto.RecordFound == RecordFoundDecision.Yes ? dto.RegisteredChildBirthDate : null; app.OperatorRemark = dto.Remark; app.SignedCertificateUploaded = dto.SignedCertificateUploaded; app.OfficerVerifiedAt = DateTime.UtcNow; app.UserName = user;
        if (dto.RecordFound == RecordFoundDecision.Yes) app.VerifiedApplicationStatus = ApplicationStatus.Approved;
        if (dto.RecordFound == RecordFoundDecision.No) app.VerifiedApplicationStatus = ApplicationStatus.Rejected;
        app.RecordLocked = false; app.RecordLockedByUserName = null; await _db.SaveChangesAsync();
        return Ok(new OfficerVerificationResultDto { Success = true, Message = "Record saved.", NewStatus = app.VerifiedApplicationStatus });
    }

    private async Task<string?> Validate(DeathApplicationFormDto f)
    {
        Normalize(f);
        if (new[] { f.ApplicantName, f.DeadPersonName, f.MotherName, f.FatherHusbandName }.Any(x => string.IsNullOrWhiteSpace(x) || NameInvalid.IsMatch(x))) return "Please enter valid names.";
        if (!Aadhar.IsMatch(f.ApplicantAadharNumber ?? "") || !Aadhar.IsMatch(f.DeadPersonAadharNumber ?? "")) return "Invalid Aadhar.";
        if (!Mobile.IsMatch(f.ApplicantMobileNumber ?? "")) return "Invalid mobile number.";
        if (string.IsNullOrWhiteSpace(f.PermanentAddress) || string.IsNullOrWhiteSpace(f.DeathPlace)) return "Please enter address and death place.";
        if (string.IsNullOrWhiteSpace(f.Email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(f.Email)) return "Invalid Email.";
        if (f.Gender is null) return "Please select gender."; if (f.DeathDate is null || f.DeathDate > DateTime.Today || f.DeathDate.Value.Year < 1927) return "Please select a valid death date.";
        if (f.ApplicantAadharNumber == f.DeadPersonAadharNumber) return "Applicant and deceased Aadhar numbers cannot be the same.";
        if (await _db.DeathApplications.CountAsync(a => a.DeadPersonAadharNumber == f.DeadPersonAadharNumber && a.DeathDate.Date == f.DeathDate.Value.Date) >= 2) return "Application already exists.";
        return null;
    }
    private static void Normalize(DeathApplicationFormDto f)
    {
        f.ApplicantName = f.ApplicantName?.Trim() ?? "";
        f.DeadPersonName = f.DeadPersonName?.Trim() ?? "";
        f.MotherName = f.MotherName?.Trim() ?? "";
        f.FatherHusbandName = f.FatherHusbandName?.Trim() ?? "";
        f.PermanentAddress = f.PermanentAddress?.Trim() ?? "";
        f.Email = f.Email?.Trim() ?? "";
        f.DeathPlace = f.DeathPlace?.Trim() ?? "";
        f.ApplicantMobileNumber = (f.ApplicantMobileNumber ?? "").Trim().Replace(" ", "").Replace("-", "");
        f.ApplicantAadharNumber = (f.ApplicantAadharNumber ?? "").Trim().Replace(" ", "").Replace("-", "");
        f.DeadPersonAadharNumber = (f.DeadPersonAadharNumber ?? "").Trim().Replace(" ", "").Replace("-", "");
    }
    private static List<RequiredDocumentDto> Documents() => new() { new() { Key = "applicantAadhar", Label = "Applicant Aadhar Card", Required = true }, new() { Key = "deathProof", Label = "Death report / proof", Required = true } };
    private static TempDeathApplication Map(DeathApplicationFormDto f, TempDeathApplication a) { a.ApplicantName=f.ApplicantName; a.ApplicantAadharNumber=f.ApplicantAadharNumber; a.ApplicantMobileNumber=f.ApplicantMobileNumber; a.PermanentAddress=f.PermanentAddress; a.Email=f.Email; a.DeadPersonName=f.DeadPersonName; a.Gender=f.Gender!.Value; a.DeathDate=f.DeathDate!.Value; a.DeathPlace=f.DeathPlace; a.DeadPersonAadharNumber=f.DeadPersonAadharNumber; a.MotherName=f.MotherName; a.FatherHusbandName=f.FatherHusbandName; return a; }
    private static void Copy(TempDeathApplication from, DeathApplication to) { to.ApplicantName=from.ApplicantName; to.ApplicantAadharNumber=from.ApplicantAadharNumber; to.ApplicantMobileNumber=from.ApplicantMobileNumber; to.PermanentAddress=from.PermanentAddress; to.Email=from.Email; to.DeadPersonName=from.DeadPersonName; to.Gender=from.Gender; to.DeathDate=from.DeathDate; to.DeathPlace=from.DeathPlace; to.DeadPersonAadharNumber=from.DeadPersonAadharNumber; to.MotherName=from.MotherName; to.FatherHusbandName=from.FatherHusbandName; to.ApplicantAadharDocUploaded=from.ApplicantAadharDocUploaded; to.DeathProofDocUploaded=from.DeathProofDocUploaded; }
    private static DeathApplicationSummaryDto Summary(DeathApplication a) => new() { Id=a.Id, ApplicationNumber=a.ApplicationNumber, DeadPersonName=a.DeadPersonName, DeathDate=a.DeathDate, DeathPlace=a.DeathPlace, ApplicantName=a.ApplicantName, ApplicantMobileNumber=a.ApplicantMobileNumber, PermanentAddress=a.PermanentAddress, VerifiedApplicationStatus=a.VerifiedApplicationStatus, PaymentMadeYesNo=a.PaymentMadeYesNo, EntryDate=a.EntryDate, CertUploadedStatus=a.RecordFound.ToString().ToUpperInvariant() };
    private static DeathApplicationDetailDto Detail(DeathApplication a) { var d = new DeathApplicationDetailDto { ApplicantAadharNumber=a.ApplicantAadharNumber, Email=a.Email, Gender=a.Gender, DeadPersonAadharNumber=a.DeadPersonAadharNumber, MotherName=a.MotherName, FatherHusbandName=a.FatherHusbandName, CrsMainetRegNo=a.CrsMainetRegNo, CrsMainetSourceValue=a.CrsMainetSourceValue, OperatorRemark=a.OperatorRemark, RegisteredDeathDate=a.RegisteredDeathDate, RecordLocked=a.RecordLocked, UploadedDocuments=new() { new() { Label="Applicant Aadhar Card", FileName=$"{a.ApplicationNumber}_ApplicantAadharCard.pdf" }, new() { Label="Death Report / Proof", FileName=$"{a.ApplicationNumber}_DeathReport.pdf" }, new() { Label="Dead Person Aadhar Card", FileName=$"{a.ApplicationNumber}_DeadPersonAadharCard.pdf" } } }; var s=Summary(a); d.Id=s.Id; d.ApplicationNumber=s.ApplicationNumber; d.DeadPersonName=s.DeadPersonName; d.DeathDate=s.DeathDate; d.DeathPlace=s.DeathPlace; d.ApplicantName=s.ApplicantName; d.ApplicantMobileNumber=s.ApplicantMobileNumber; d.PermanentAddress=s.PermanentAddress; d.VerifiedApplicationStatus=s.VerifiedApplicationStatus; d.PaymentMadeYesNo=s.PaymentMadeYesNo; d.EntryDate=s.EntryDate; d.CertUploadedStatus=s.CertUploadedStatus; return d; }
}
