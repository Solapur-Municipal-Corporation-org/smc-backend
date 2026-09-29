using SMC.Master.Application.DTOs.Citizen;

namespace SMC.Master.Application.Interfaces;

public interface ICitizenService
{
    Task<CitizenDto?> GetByUserIdAsync(Guid userId);
    Task<Guid> RegisterAsync(RegisterCitizenDto request);
    Task UpdateAsync(Guid citizenId, CitizenDto request);
}
