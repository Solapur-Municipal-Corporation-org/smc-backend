using System.Text.RegularExpressions;
using BDMS.Application.DTOs;
using BDMS.Application.Services;
using BDMS.Domain.Models;
using BDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Infrastructure.Services;

/// <summary>
/// Direct port of BDMS_Page_Online.aspx.cs: validateData() + validateDuplicateName1() +
/// the fee/penalty/ackSubject calculation block inside btnInsert_Click(). Order of checks
/// is preserved exactly (including the quirk where navnondni=="Yes" resets Penalty to 0
/// even if IsAppliedAfterFifteenYears=="Yes" set it to 5 earlier — that's what the legacy
/// code does, so it's kept as-is here rather than "fixed").
/// </summary>
public class BirthApplicationValidator : IBirthApplicationValidator
{
    private static readonly Regex ForbiddenNameChars = new(@"[0-9@#$!%^&*]");
    private static readonly Regex MarathiFieldForbidden = new(@"[0-9a-zA-Z@#$!%^&*]");
    private static readonly Regex MobileRegex = new(@"^[0-9]{10}$");
    private static readonly Regex AadharRegex = new(@"^\d{12}$");
    private static readonly Regex EmailRegex = new(@"^([\w-.]+)@([\w-]+\.)+[a-zA-Z]{2,4}$");

    private readonly BdmsDbContext _db;

    public BirthApplicationValidator(BdmsDbContext db) => _db = db;

    public async Task<BirthValidationResult> ValidateAsync(BirthApplicationFormDto dto)
    {
        // ---- validateData() ----
        if (ForbiddenNameChars.IsMatch(dto.FatherName ?? ""))
            return BirthValidationResult.Fail("Please enter valid Father name.");

        if (ForbiddenNameChars.IsMatch(dto.MotherName ?? ""))
            return BirthValidationResult.Fail("Please enter valid Mother name.");

        if (string.IsNullOrWhiteSpace(dto.ChildNameMarathi))
            return BirthValidationResult.Fail("Please enter valid Child name in marathi.");

        if (string.IsNullOrWhiteSpace(dto.ChildNameEnglish))
            return BirthValidationResult.Fail("Please enter valid Child name in english.");

        if (ForbiddenNameChars.IsMatch(dto.ChildNameEnglish))
            return BirthValidationResult.Fail("Please enter valid Child name in english.");

        if (MarathiFieldForbidden.IsMatch(dto.ChildNameMarathi))
            return BirthValidationResult.Fail("Please enter valid Child name in marathi.");

        var fatherMobile = dto.FatherMobileNumber?.Trim() ?? "";
        var motherMobile = dto.MotherMobileNumber?.Trim() ?? "";

        if (fatherMobile == "" && motherMobile == "")
            return BirthValidationResult.Fail("Please enter valid mobile number of father or mother.");

        if (fatherMobile != "" && !MobileRegex.IsMatch(fatherMobile))
            return BirthValidationResult.Fail("Please enter valid mobile number of father.");

        if (motherMobile != "" && !MobileRegex.IsMatch(motherMobile))
            return BirthValidationResult.Fail("Please enter valid mobile number of mother.");

        if (string.IsNullOrWhiteSpace(dto.PermanentAddress))
            return BirthValidationResult.Fail("Please enter address.");

        var fatherAadhar = dto.FatherAadharNumber?.Trim() ?? "";
        var motherAadhar = dto.MotherAadharNumber?.Trim() ?? "";

        if (fatherAadhar == "" && motherAadhar == "")
            return BirthValidationResult.Fail("Please enter father or mother aadhar card.");

        if (fatherAadhar != "" && !AadharRegex.IsMatch(fatherAadhar))
            return BirthValidationResult.Fail("Invalid Aadhar");

        if (motherAadhar != "" && !AadharRegex.IsMatch(motherAadhar))
            return BirthValidationResult.Fail("Invalid Aadhar");

        if (string.IsNullOrWhiteSpace(dto.FatherEmail))
            return BirthValidationResult.Fail("Please enter email.");

        if (!EmailRegex.IsMatch(dto.FatherEmail))
            return BirthValidationResult.Fail("Invalid Email");

        if (dto.ChildGender is null)
            return BirthValidationResult.Fail("Please Select Gendar.");

        if (dto.ChildBirthDate is null)
            return BirthValidationResult.Fail("Please select birth date.");

        var birthDate = dto.ChildBirthDate.Value;
        if (birthDate.Year < 1927)
            return BirthValidationResult.Fail("Please select year greater than 1927.");
        if (birthDate > DateTime.Now)
            return BirthValidationResult.Fail("Please select birth date today or less than today.");

        // ---- validateDuplicateName1() ----
        if (motherAadhar != "" && fatherAadhar != "" && motherAadhar == fatherAadhar)
            return BirthValidationResult.Fail("Father Aadhar number and Mother Aadhar number can not be same.");

        var fatherNameNoSpace = (dto.FatherName ?? "").Replace(" ", "");
        var motherNameNoSpace = (dto.MotherName ?? "").Replace(" ", "");
        if (fatherNameNoSpace == motherNameNoSpace)
            return BirthValidationResult.Fail("Please Father name and Mother name is not valid.");

        // ---- ddlAfterFifteenYears / ddlNavNondni gating ----
        if (dto.IsAppliedAfterFifteenYears is null or YesNoSelect.NotSelected)
            return BirthValidationResult.Fail("Please select 'जन्मतारखे पासून १५ वर्षे वया नंतर नाव नोंदवायचे आहेत का ?'.");

        if (dto.IsAppliedAfterFifteenYears == YesNoSelect.No &&
            (dto.Navnondni is null or YesNoSelect.NotSelected))
            return BirthValidationResult.Fail("Please select 'याआधी बाळाचे नाव महापालिकेस कळविले आहे का ?'.");

        // ---- fee / penalty calculation (exact legacy order, quirks preserved) ----
        decimal amount = 0;
        decimal penalty = 0;

        if (dto.IsAppliedAfterFifteenYears == YesNoSelect.Yes)
        {
            amount = 30;
            penalty = 5;
        }

        string ackSub;
        if (dto.Navnondni == YesNoSelect.No)
        {
            var days = (DateTime.Now - birthDate).Days;
            if (days > 5475)
                return BirthValidationResult.Fail("Can not inform Child name after 15 years from Birth Date.");
            if (days < 0)
                return BirthValidationResult.Fail("Pl. select valid birth date");

            if (string.IsNullOrWhiteSpace(dto.VartaNumber))
                return BirthValidationResult.Fail("Please enter varta number (जन्म अहवाल क्रमांक).");

            amount = 30;
            if (days > 365) penalty = 5;

            ackSub = "बाळाचे नाव कळविण्याचे अर्ज / Child Name Registration.";
        }
        else if (dto.IsAppliedAfterFifteenYears == YesNoSelect.Yes)
        {
            ackSub = "बाळाचे नाव १५ वर्षा नंतर नोंदविण्याचे अर्ज / Child Name Registration After 15 Year.";
        }
        else
        {
            ackSub = "जन्म दाखला / Birth Certificate";
        }

        if (dto.Navnondni == YesNoSelect.Yes)
        {
            amount = 30;
            penalty = 0; // legacy quirk: unconditionally resets penalty, even over the after-15-years case above
        }

        // ---- duplicate application check (validateMobileNumberNameBirthDate: >=2 exact matches) ----
        var childNameUpper = dto.ChildNameEnglish.ToUpperInvariant().Trim();
        var duplicateCount = await _db.BirthApplications.CountAsync(a =>
            a.EntryDate.Date > new DateTime(2024, 3, 31) &&
            a.ChildNameEnglish.ToUpper() == childNameUpper &&
            a.ChildBirthDate.Date == birthDate.Date &&
            ((fatherMobile != "" && a.FatherMobileNumber == fatherMobile) ||
             (motherMobile != "" && a.MotherMobileNumber == motherMobile)));

        if (duplicateCount >= 2)
            return BirthValidationResult.Fail("Application is already exist.");

        // ---- fraud check (validateMobileNumber: same mobile/email used more than 5 times) ----
        if (fatherMobile != "")
        {
            var usedCount = await _db.BirthApplications.CountAsync(a =>
                a.EntryDate.Date > new DateTime(2024, 3, 31) && a.FatherMobileNumber == fatherMobile);
            if (usedCount > 5)
                return BirthValidationResult.Fail("This mobile number is used for more than 5 times. Please use different mobile number.");
        }
        if (motherMobile != "")
        {
            var usedCount = await _db.BirthApplications.CountAsync(a =>
                a.EntryDate.Date > new DateTime(2024, 3, 31) && a.MotherMobileNumber == motherMobile);
            if (usedCount > 5)
                return BirthValidationResult.Fail("This mobile number is used for more than 5 times. Please use different mobile number.");
        }
        if (!string.IsNullOrWhiteSpace(dto.FatherEmail))
        {
            var usedCount = await _db.BirthApplications.CountAsync(a =>
                a.EntryDate.Date > new DateTime(2024, 3, 31) && a.FatherEmail == dto.FatherEmail);
            if (usedCount > 5)
                return BirthValidationResult.Fail("This email id is used for more than 5 times. Please use different email id.");
        }

        return new BirthValidationResult
        {
            IsValid = true,
            DakhalaFee = amount,
            Penalty = penalty,
            AckSubject = ackSub
        };
    }
}
