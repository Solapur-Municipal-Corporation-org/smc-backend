using System.ComponentModel.DataAnnotations;

namespace CitizenPortal.Api.Models;

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameMarathi { get; set; }

    [MaxLength(50)]
    public string IconName { get; set; } = "Landmark";

    [MaxLength(50)]
    public string DepartmentDescription { get; set; } = "Citizen";

    /// <summary>Controls the order departments appear in the sidebar/API response (ascending). Lower = higher up.</summary>
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Service> Services { get; set; } = new List<Service>();
}
