using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public enum FieldType { Text, Number, Date, Select, Textarea, Email, Tel }

public class ServiceField
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceId { get; set; }
    public Service? Service { get; set; }

    [Required, MaxLength(150)]
    public string Label { get; set; } = string.Empty;

    public FieldType FieldType { get; set; } = FieldType.Text;

    public bool Required { get; set; } = true;

    /// <summary>Comma-separated list of options, used only when FieldType = Select</summary>
    [MaxLength(1000)]
    public string? Options { get; set; }

    public int DisplayOrder { get; set; }
}
