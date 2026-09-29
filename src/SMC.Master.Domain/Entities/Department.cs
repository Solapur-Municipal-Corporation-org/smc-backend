namespace SMC.Master.Domain.Entities;

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NameEn { get; set; } = default!;
    public string NameMr { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public ICollection<DepartmentService> DepartmentServices { get; set; } = new List<DepartmentService>();
}
