using SMC.Master.Application.DTOs.Application;

namespace SMC.Master.Application.Interfaces;

public interface IApplicationService
{
    Task<List<ApplicationDto>> GetForCitizenAsync(Guid citizenId);
    Task<ApplicationDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Guid citizenId, Guid serviceId, string formDataJson);
    Task UpdateStatusAsync(Guid id, string status);
}
