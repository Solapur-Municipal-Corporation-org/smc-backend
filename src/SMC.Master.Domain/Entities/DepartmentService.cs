namespace SMC.Master.Domain.Entities;

// Join entity: which services a given department offers.
public class DepartmentService
{
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = default!;
}
