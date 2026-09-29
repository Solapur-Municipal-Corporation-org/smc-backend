using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public enum PaymentStatus { Initiated, Success, Failed }

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApplicationId { get; set; }
    public Application? Application { get; set; }

    [Required, MaxLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(30)]
    public string PaymentMode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;

    public DateTime? PaidOn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
