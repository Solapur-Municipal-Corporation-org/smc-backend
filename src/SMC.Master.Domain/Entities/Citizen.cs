namespace SMC.Master.Domain.Entities;

public class Citizen
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public DateOnly Dob { get; set; }
    public string Gender { get; set; } = default!; // Male / Female / Other
    public string AadhaarNumber { get; set; } = default!;
    public string MobileNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
