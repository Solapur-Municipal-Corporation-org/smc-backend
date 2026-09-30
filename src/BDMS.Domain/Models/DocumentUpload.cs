using System.ComponentModel.DataAnnotations;

namespace BDMS.Domain.Models;

public class DocumentUpload
{
    [Key]
    public int Id { get; set; }
    [MaxLength(100)]
    public string TempApplicationNumber { get; set; } = string.Empty;
    [MaxLength(50)]
    public string DocumentKey { get; set; } = string.Empty;
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}