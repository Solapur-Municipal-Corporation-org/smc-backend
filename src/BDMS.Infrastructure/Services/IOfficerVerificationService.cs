using System.Text.RegularExpressions;
using BDMS.Application.DTOs;
using BDMS.Application.Services;
using BDMS.Domain.Models;
using BDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Infrastructure.Services;

/// <summary>
/// Direct port of BDMS_Page.aspx.cs btnInsert_Click() + updateResponse() + updateResponse11().
/// This is the real "CRS -Mainet" cross-check that officers perform against physical/archive
/// records before a Birth application is Approved/Rejected — replaces the placeholder
/// Approve/Reject buttons from the earlier build. Validation order and status-mapping rules
/// (including the SubRegistrar shortcut that skips all other validation) are preserved exactly.
/// </summary>
public class OfficerVerificationService : IOfficerVerificationService
{
    private static readonly Regex ForbiddenRemarkChars = new(@"[@#$!%^&*]");

    private readonly BdmsDbContext _db;
    private readonly IOtpService _sms; // reusing the dummy "send" logger for SMS, same pattern as OTP

    public OfficerVerificationService(BdmsDbContext db, IOtpService sms)
    {
        _db = db;
        _sms = sms;
    }

    public async Task<OfficerVerificationResultDto> SubmitAsync(
        string applicationNumber, string officerUserName, OfficerVerificationDto dto)
    {
        var app = await _db.BirthApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        if (app == null)
            return new OfficerVerificationResultDto { Success = false, Message = "Application not found." };

        if (!app.RecordLocked || app.RecordLockedByUserName != officerUserName)
            return new OfficerVerificationResultDto { Success = false, Message = "Open this application for review first (record must be locked by you)." };

        // ---- validation, exact legacy order ----
        if (dto.RecordFound == RecordFoundDecision.Pending)
            return new OfficerVerificationResultDto { Success = false, Message = "Please select record found or not." };

        // SubRegistrar: legacy shortcut -- skips every other validation, saves, and returns immediately.
        if (dto.RecordFound == RecordFoundDecision.SubRegistrar)
        {
            app.RecordFound = RecordFoundDecision.SubRegistrar;
            app.UserName = officerUserName;
            ReleaseLock(app);
            await _db.SaveChangesAsync();
            return new OfficerVerificationResultDto { Success = true, Message = "Record Saved!!." };
        }

        if (dto.RecordFound == RecordFoundDecision.Yes)
        {
            if (string.IsNullOrWhiteSpace(dto.CrsMainetRegNo))
                return new OfficerVerificationResultDto { Success = false, Message = "Please enter CRS or Mainet Registration Number." };
            if (dto.CrsMainetSourceValue == CrsMainetSource.NotSelected)
                return new OfficerVerificationResultDto { Success = false, Message = "Please select record found in CRS or net." };
            if (dto.IsNamePreviouslyReportedOperatorVerified == YesNoSelect.NotSelected)
                return new OfficerVerificationResultDto { Success = false, Message = "Please select - याआधी बाळाचे नाव महापालिकेस कळविले आहे का ?" };
        }

        if (!string.IsNullOrEmpty(dto.Remark) && ForbiddenRemarkChars.IsMatch(dto.Remark))
            return new OfficerVerificationResultDto { Success = false, Message = "Please enter valid remark." };

        if (dto.RecordFound == RecordFoundDecision.Yes &&
            (!app.SignedCertificateUploaded || app.IssuedCertificateContent is not { Length: > 0 }))
            return new OfficerVerificationResultDto { Success = false, Message = "Please select document to upload." };

        DateTime? registeredDate = dto.RegisteredChildBirthDate;
        var registeredChildName = dto.RegisteredChildName;

        if (dto.RecordFound == RecordFoundDecision.Yes && dto.IsNamePreviouslyReportedOperatorVerified == YesNoSelect.Yes)
        {
            if (registeredDate is null)
                return new OfficerVerificationResultDto { Success = false, Message = "Please select Child Birth Registered date." };
            if (string.IsNullOrWhiteSpace(registeredChildName))
                return new OfficerVerificationResultDto { Success = false, Message = "Please enter Registered Child Name." };
        }
        else if (dto.RecordFound == RecordFoundDecision.Yes && dto.IsNamePreviouslyReportedOperatorVerified == YesNoSelect.No)
        {
            // safety-net auto-fill: registered date = child's birth date, registered name = the child's name on file
            registeredDate = app.ChildBirthDate;
            registeredChildName = app.ChildNameEnglish;
        }
        else
        {
            registeredDate = null;
            registeredChildName = null;
        }

        // ---- status mapping, exact port of updateResponse() ----
        ApplicationStatus vefStatus = app.VerifiedApplicationStatus; // legacy default: stays "Pending" unless set below
        bool docVerified = false;
        DateTime? assignedToAbhilekhapalDate = null;

        switch (dto.RecordFound)
        {
            case RecordFoundDecision.No:
                vefStatus = ApplicationStatus.Rejected;
                docVerified = true;
                break;
            case RecordFoundDecision.Yes:
                vefStatus = ApplicationStatus.Approved;
                docVerified = true;
                break;
            case RecordFoundDecision.Abhilekhapal:
                assignedToAbhilekhapalDate = DateTime.UtcNow;
                break;
            default:
                break;
        }

        app.RecordFound = dto.RecordFound;
        app.RegisteredChildName = registeredChildName;
        app.RegisteredChildBirthDate = registeredDate;
        app.CrsMainetRegNo = dto.CrsMainetRegNo;
        app.OfficerVerifiedAt = DateTime.UtcNow;
        app.IsNamePreviouslyReportedOperatorVerified = dto.IsNamePreviouslyReportedOperatorVerified;
        app.IsNamePreviouslyReportedByCitizen ??= app.Navnondni switch
        {
            YesNoSelect.Yes => true,
            YesNoSelect.No => false,
            _ => null
        };
        app.CrsMainetSourceValue = dto.CrsMainetSourceValue;
        app.OperatorRemark = dto.Remark;
        app.DocVerified = docVerified;
        app.VerifiedApplicationStatus = vefStatus;
        app.AssignedToAbhilekhapalDate = assignedToAbhilekhapalDate;
        // The certificate is marked uploaded only by the authenticated multipart upload endpoint.
        // Do not trust the JSON checkbox value from the verification request.
        app.UserName = officerUserName;

        ReleaseLock(app);
        await _db.SaveChangesAsync();

        // ---- post-save SMS, exact port of the legacy branches (dummy: logged, not actually sent) ----
        string message;
        if (dto.RecordFound == RecordFoundDecision.Yes)
        {
            await _sms.SendOtpAsync(applicationNumber, app.FatherMobileNumber,
                "Approved -- please make payment and download your certificate.");
            message = "Record saved.";
        }
        else if (dto.RecordFound == RecordFoundDecision.No)
        {
            await _sms.SendOtpAsync(applicationNumber, app.FatherMobileNumber,
                "Rejected -- registration record not found. Please contact the Birth & Death Dept. with your Birth Certificate (if previously taken) or Birth information form.");
            message = "Record saved.";
        }
        else
        {
            message = "Record saved.";
        }

        return new OfficerVerificationResultDto { Success = true, Message = message, NewStatus = app.VerifiedApplicationStatus };
    }

    private static void ReleaseLock(BirthApplication app)
    {
        app.RecordLocked = false;
        app.RecordLockedByUserName = null;
        app.RecordLockedAt = null;
    }
}
