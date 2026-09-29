namespace CitizenPortal.Api.Department.DTOs;

public class LoginRequestDto
{
    public string MobileNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class OrganizationDto
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
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public int OrganizationId { get; set; }
    public int SrNo { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentNameMarathi { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public string PrimaryFunctions { get; set; } = string.Empty;
    public string? DepartmentHead { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public int ServiceCount { get; set; }
}

public class UserDto
{
    public int UserId { get; set; }
    public int? DepartmentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = "DepartmentUser"; // "Admin" or "DepartmentUser"
    public bool IsActive { get; set; } = true;
    /// <summary>Plain-text password from the entry form. Only sent when setting/changing a password —
    /// left blank on Update to keep the existing password. Never returned by the API.</summary>
    public string? Password { get; set; }
}

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
