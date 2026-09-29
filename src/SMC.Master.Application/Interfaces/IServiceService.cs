using SMC.Master.Application.DTOs.Service;

namespace SMC.Master.Application.Interfaces;

public interface IServiceService
{
    Task<List<ServiceDto>> GetAllAsync();
    Task<List<ServiceDto>> GetByDepartmentAsync(Guid departmentId);
}
