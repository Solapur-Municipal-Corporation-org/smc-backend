using SMC.Master.Application.DTOs.Citizen;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.Application.Services;

public class CitizenService : ICitizenService
{
    public Task<CitizenDto?> GetByUserIdAsync(Guid userId) => throw new NotImplementedException();
    public Task<Guid> RegisterAsync(RegisterCitizenDto request) => throw new NotImplementedException();
    public Task UpdateAsync(Guid citizenId, CitizenDto request) => throw new NotImplementedException();
}
