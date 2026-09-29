namespace SMC.Master.Domain.Entities;

public class ApplicationStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!; // Draft, Submitted, UnderReview, Approved, Rejected, Completed
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
