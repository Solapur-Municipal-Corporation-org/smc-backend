namespace SMC.Master.Domain.Entities;

public class Application
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = default!;
    public Guid CitizenId { get; set; }
    public Citizen Citizen { get; set; } = default!;
    public Guid StatusId { get; set; }
    public ApplicationStatus Status { get; set; } = default!;
    public string FormDataJson { get; set; } = "{}";
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
