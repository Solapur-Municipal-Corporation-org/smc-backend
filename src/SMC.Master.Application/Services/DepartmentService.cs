using SMC.Master.Application.DTOs.Department;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.Application.Services;

public class DepartmentService : IDepartmentService
{
    public Task<List<DepartmentDto>> GetAllAsync() => throw new NotImplementedException();
    public Task<DepartmentDto?> GetByIdAsync(Guid id) => throw new NotImplementedException();
}
