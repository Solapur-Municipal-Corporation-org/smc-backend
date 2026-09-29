namespace CitizenPortal.Api.DTOs;

public class PaymentInitiateRequest
{
    public Guid ApplicationId { get; set; }
}

public class PaymentConfirmRequest
{
    public Guid ApplicationId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
}

public class PaymentReceiptResponse
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public string ApplicationNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaidOn { get; set; }
    public string PaymentMode { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}
