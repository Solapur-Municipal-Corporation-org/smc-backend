using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public class ApplicationDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApplicationId { get; set; }
    public Application? Application { get; set; }

    [Required, MaxLength(200)]
    public string DocumentName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
}
