namespace CitizenPortal.Api.Department.Entities;

// NOTE ON THIS FILE (integration change — see INTEGRATION_REPORT.md item "Users/Employees/Departments"):
// StaffDepartment/AppUser/Employee below map via EF configuration to the *already-existing*
// `Departments`/`Users`/`Employees` tables in smc_db (int-keyed) — the same tables the standalone
// Department Portal package created. They are deliberately NOT merged into CitizenPortal.Api's
// Guid-keyed `Department` (-> MR_DEPT_Departments) to avoid a destructive primary-key-type
// migration on live data. The two "Department" concepts are linked at query time by matching
// StaffDepartment.DepartmentCode == Department.Code — see IDepartmentLinkService.

/// <summary>Root master. Every department, user and service is scoped to an organization.</summary>
public class Organization
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationNameMarathi { get; set; } = string.Empty;
    public string? PrintingHeader { get; set; }
    public string? OrganizationType { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber1 { get; set; }
    public string? PhoneNumber2 { get; set; }
    public string? Website { get; set; }
    public string? GstNumber { get; set; }
    public string? PanNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? ActiveCommissioner { get; set; }
    public string? ActiveMayor { get; set; }
    public string? ActiveDyMayor { get; set; }
    public string? ActiveStandingChairman { get; set; }
    public string? EmailId1 { get; set; }
    public string? EmailId2 { get; set; }
    public string? Location { get; set; }
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<StaffDepartment> Departments { get; set; } = new List<StaffDepartment>();
}

public class StaffDepartment
{
    public int DepartmentId { get; set; }
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public int SrNo { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentNameMarathi { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public string PrimaryFunctions { get; set; } = string.Empty;
    public string? DepartmentHead { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<StaffService> Services { get; set; } = new List<StaffService>();
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}

public class StaffService
{
    public int ServiceId { get; set; }
    public int DepartmentId { get; set; }
    public StaffDepartment? Department { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceNameMarathi { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Integration change: enum MEMBER NAMES renamed to match the spec's vocabulary; the underlying
// int values are UNCHANGED, so existing rows in the live Users table (Role = 1 or 2) still parse
// correctly — this is a source-only rename, not a data migration. "DepartmentAdmin" is new (4) and
// requires no data change; nobody has it yet until you promote a user.
public enum UserRole
{
    SystemAdmin = 1,       // was "Admin" — org-wide access (item 7: SystemAdmin/MasterAdmin)
    DepartmentEmployee = 2, // was "DepartmentUser" — same stored value, renamed only
    DepartmentAdmin = 4,    // new — full access within their own department only
}

public class AppUser
{
    public int UserId { get; set; }
    public int? DepartmentId { get; set; }
    public StaffDepartment? Department { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.DepartmentEmployee;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ---- Added for OTP login (item 5). Additive/nullable columns — see migration script. ----
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public int OtpAttemptCount { get; set; } = 0;
}

/// <summary>Employee master — fields match the "Employee" sheet in organization_Master.xlsx.</summary>
public class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public StaffDepartment? Department { get; set; }
    public string FirstNameEnglish { get; set; } = string.Empty;
    public string? FirstNameMarathi { get; set; }
    public string? FatherNameEnglish { get; set; }
    public string? FatherNameMarathi { get; set; }
    public string? MotherNameEnglish { get; set; }
    public string? MotherNameMarathi { get; set; }
    public string LastNameEnglish { get; set; } = string.Empty;
    public string? LastNameMarathi { get; set; }
    public string? MobileNumber { get; set; }
    public string? MailId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime? Dob { get; set; }
    public string? AadharNo { get; set; }
    public string? PanNo { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Occupation { get; set; }
    public bool PhysicallyHandicapped { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Education> Educations { get; set; } = new List<Education>();
    public ICollection<Family> FamilyMembers { get; set; } = new List<Family>();
    public ICollection<EmployeeLeaveBalance> LeaveBalances { get; set; } = new List<EmployeeLeaveBalance>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();

    // One-to-one profile sections used by the combined Employee wizard form.
    public EmployeeAddress? Address { get; set; }
    public EmployeeBankDetail? BankDetail { get; set; }
    public EmployeeSalary? Salary { get; set; }
}

/// <summary>
/// Employee's addresses — child of Employee (one-to-one row holding BOTH addresses).
/// Permanent* fields are the permanent/native address; Current* fields are the present
/// residential address. SameAsPermanent is a UI convenience flag: when true the Current*
/// fields are kept mirrored to the Permanent* fields.
/// </summary>
public class EmployeeAddress
{
    public int EmployeeAddressId { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    // ---- Permanent Address ----
    public string PermanentAddressLine1 { get; set; } = string.Empty;
    public string? PermanentAddressLine2 { get; set; }
    public string? PermanentCity { get; set; }
    public string? PermanentTahsil { get; set; }
    public string? PermanentDistrict { get; set; }
    public string? PermanentState { get; set; }
    public string? PermanentCountry { get; set; }
    public string? PermanentPincode { get; set; }

    // ---- Current Address ----
    public bool SameAsPermanent { get; set; } = false;
    public string? CurrentAddressLine1 { get; set; }
    public string? CurrentAddressLine2 { get; set; }
    public string? CurrentCity { get; set; }
    public string? CurrentTahsil { get; set; }
    public string? CurrentDistrict { get; set; }
    public string? CurrentState { get; set; }
    public string? CurrentCountry { get; set; }
    public string? CurrentPincode { get; set; }
}

/// <summary>Employee's salary bank account — child of Employee (one-to-one).</summary>
public class EmployeeBankDetail
{
    public int EmployeeBankDetailId { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? AccountHolderName { get; set; }
    public string? AccountNumber { get; set; }
    public string? IfscCode { get; set; }
    public string? AccountType { get; set; } // Savings / Current
}

/// <summary>Employee's current pay structure — child of Employee (one-to-one).</summary>
public class EmployeeSalary
{
    public int EmployeeSalaryId { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string? PayScale { get; set; }
    public decimal? BasicPay { get; set; }
    public decimal? GradePay { get; set; }
    public decimal? DearnessAllowance { get; set; }
    public decimal? HouseRentAllowance { get; set; }
    public decimal? OtherAllowance { get; set; }
    public decimal? GrossSalary { get; set; }
    public decimal? TotalDeductions { get; set; }
    public decimal? NetSalary { get; set; }
    public DateTime? EffectiveFrom { get; set; }
}
public class Education
{
    public int EducationId { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string EducationName { get; set; } = string.Empty; // "Education" field
    public string? BoardUniversity { get; set; }
    public string? Year { get; set; }
    public string? Marks { get; set; }
    public string? Percentage { get; set; }
    public string? State { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>Family sheet — child of Employee.</summary>
public class Family
{
    public int FamilyId { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string RelationType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? BloodGroup { get; set; }
    public int? Age { get; set; }
    public bool Pension { get; set; } = false;
    public decimal? PensionPercentage { get; set; }
}

/// <summary>
/// Leave sheet — per organization_Master.xlsx this lists leave-type balances (EL, HPL, etc.)
/// held per employee per financial year, not a standalone leave-type master.
/// </summary>
public class EmployeeLeaveBalance
{
    public int LeaveBalanceId { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int? FinancialYearId { get; set; }
    public FinancialYear? FinancialYear { get; set; }
    public int EarnedLeave { get; set; }
    public int HalfPayLeave { get; set; }
    public int CommutedLeave { get; set; }
    public int LeaveNotDue { get; set; }
    public int MaternityLeave { get; set; }
    public int PaternityLeave { get; set; }
    public int ChildCareLeave { get; set; }
    public int StudyLeave { get; set; }
    public int CasualLeave { get; set; }
    public int ExtraordinaryLeave { get; set; }
    public int SpecialDisabilityLeave { get; set; }
    public int HospitalLeave { get; set; }
    public int QuarantineLeave { get; set; }
}

/// <summary>Document sheet — child of Employee. FilePath/FileName/ContentType are filled in
/// by the /api/uploads/employee-document endpoint after the file is uploaded.</summary>
public class EmployeeDocument
{
    public int DocumentId { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string? FilePath { get; set; } // "Upload File" — relative URL under /uploads/...
    public string? FileName { get; set; } // original file name, for display
    public string? ContentType { get; set; } // e.g. image/jpeg, application/pdf
    public long? FileSizeBytes { get; set; }
    public string? Remarks { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class CitizenApplication
{
    public int ApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public StaffDepartment? Department { get; set; }
    public int ServiceId { get; set; }
    public StaffService? Service { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string? ApplicantMobile { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Approved, Rejected
    public string? Remarks { get; set; }
    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedOn { get; set; }
}

public class AuditLog
{
    public int AuditLogId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
