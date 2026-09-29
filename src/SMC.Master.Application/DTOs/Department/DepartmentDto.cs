namespace SMC.Master.Application.DTOs.Department;

public record DepartmentDto(Guid Id, string NameEn, string NameMr, string Code, string? Description, string? IconUrl);
