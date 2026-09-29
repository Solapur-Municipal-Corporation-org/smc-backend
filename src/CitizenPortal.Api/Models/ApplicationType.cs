using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public class ApplicationType
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceId { get; set; }
    public Service? Service { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<ApplicantType> ApplicantTypes { get; set; } = new List<ApplicantType>();
}
