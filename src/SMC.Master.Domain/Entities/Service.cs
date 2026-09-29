namespace SMC.Master.Domain.Entities;

public class Service
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NameEn { get; set; } = default!;
    public string NameMr { get; set; } = default!;
    public bool IsIntegrated { get; set; }
    public string? IntegrationKey { get; set; } // service-a / service-b / service-c
    public string RequiredDocumentsCsv { get; set; } = string.Empty;
    public ICollection<DepartmentService> DepartmentServices { get; set; } = new List<DepartmentService>();
}
