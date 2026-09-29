using SMC.Master.Application.DTOs.Service;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.Application.Services;

public class ServiceService : IServiceService
{
    public Task<List<ServiceDto>> GetAllAsync() => throw new NotImplementedException();
    public Task<List<ServiceDto>> GetByDepartmentAsync(Guid departmentId) => throw new NotImplementedException();
}
