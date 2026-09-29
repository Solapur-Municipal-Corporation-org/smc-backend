namespace CitizenPortal.Api.DTOs;

public class ServiceFieldResponse
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public List<string>? Options { get; set; }
}

public class ServiceSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public int ProcessingDays { get; set; }
}

public class ServiceDetailResponse : ServiceSummaryResponse
{
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public List<string> DocumentsRequired { get; set; } = new();
    public List<ServiceFieldResponse> Fields { get; set; } = new();
}

public class DepartmentResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public List<ServiceSummaryResponse> Services { get; set; } = new();
}

public class ApplicationTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ApplicantTypeResponse> ApplicantTypes { get; set; } = new();
}

public class ApplicantTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

