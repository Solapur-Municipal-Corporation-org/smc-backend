namespace CitizenPortal.Api.DTOs;

public class ApplicationCreateRequest
{
    public Guid ServiceId { get; set; }

    /// <summary>Dynamic field values keyed by ServiceField.Id or Label</summary>
    public Dictionary<string, string> FormData { get; set; } = new();
}

public class ApplicationResponse
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public string PaymentStatus { get; set; } = "Unpaid";
    public string FormDataJson { get; set; } = "{}";
    public DateTime SubmittedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? Remarks { get; set; }
}
