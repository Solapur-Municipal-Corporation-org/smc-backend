using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public class Certificate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApplicationId { get; set; }
    public Application? Application { get; set; }

    [Required, MaxLength(10)]
    public string FinancialYear { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public DateTime IssuedOn { get; set; } = DateTime.UtcNow;
}
