using System.ComponentModel.DataAnnotations;

namespace BDMS.Domain.Models;

/// <summary>Mirrors the effect of legacy Responce.aspx callback (Payment_Made_Yes_No, Transaction_Status).</summary>
public class PaymentRecord
{
    [Key]
    public int Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = "Initiated"; // Initiated | Success | Failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDummy { get; set; } = true;
}
