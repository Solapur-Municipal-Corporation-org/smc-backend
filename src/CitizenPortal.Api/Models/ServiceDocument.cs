using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public class ServiceDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceId { get; set; }
    public Service? Service { get; set; }

    [Required, MaxLength(200)]
    public string DocumentName { get; set; } = string.Empty;
}
