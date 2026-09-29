using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

// Integration change (additive only — existing int values 0-3 are unchanged, so no data
// migration needed): added Level1Scrutiny/Level2Verification at the end so the Department
// Portal's review workflow (spec item 10) can track which stage an application is at.
// "UnderReview" is kept for any already-stored rows that predate this distinction.
public enum ApplicationStatus { Pending, UnderReview, Approved, Rejected, Level1Scrutiny, Level2Verification }

public class Application
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string ApplicationNumber { get; set; } = string.Empty;

    public Guid CitizenId { get; set; }
    public Citizen? Citizen { get; set; }

    public Guid ServiceId { get; set; }
    public Service? Service { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    [Required, MaxLength(10)]
    public string FinancialYear { get; set; } = string.Empty;

    /// <summary>JSON-serialized key/value pairs of the dynamic form submitted by the citizen.</summary>
    public string FormDataJson { get; set; } = "{}";

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
    public Payment? Payment { get; set; }
    public Certificate? Certificate { get; set; }
}
