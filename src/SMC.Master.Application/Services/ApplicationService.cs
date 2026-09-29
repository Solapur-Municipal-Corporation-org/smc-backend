using SMC.Master.Application.DTOs.Application;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.Application.Services;

public class ApplicationService : IApplicationService
{
    public Task<List<ApplicationDto>> GetForCitizenAsync(Guid citizenId) => throw new NotImplementedException();
    public Task<ApplicationDto?> GetByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<Guid> CreateAsync(Guid citizenId, Guid serviceId, string formDataJson) => throw new NotImplementedException();
    public Task UpdateStatusAsync(Guid id, string status) => throw new NotImplementedException();
}
