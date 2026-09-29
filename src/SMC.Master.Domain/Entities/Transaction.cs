namespace SMC.Master.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Mode { get; set; } = default!; // Online / Cash / DD / Cheque
    public string Status { get; set; } = default!; // Pending / Success / Failed
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string ReferenceNumber { get; set; } = default!;
}
