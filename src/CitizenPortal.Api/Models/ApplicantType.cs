using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public class ApplicantType
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApplicationTypeId { get; set; }
    public ApplicationType? ApplicationType { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
}
