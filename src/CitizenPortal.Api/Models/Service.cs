using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public class Service
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(60)]
    public string ServiceCode { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? NameMarathi { get; set; }

    public decimal Fee { get; set; }

    public int ProcessingDays { get; set; } = 7;

    public bool IsActive { get; set; } = true;

    public ICollection<ServiceField> Fields { get; set; } = new List<ServiceField>();
    public ICollection<ServiceDocument> RequiredDocuments { get; set; } = new List<ServiceDocument>();
    public ICollection<ApplicationType> ApplicationTypes { get; set; } = new List<ApplicationType>();
}
