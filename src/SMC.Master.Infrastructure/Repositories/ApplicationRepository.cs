using Microsoft.EntityFrameworkCore;
using SMC.Master.Domain.Entities;
using SMC.Master.Infrastructure.Data;

namespace SMC.Master.Infrastructure.Repositories;

public class ApplicationRepository
{
    private readonly MasterDbContext _db;
    public ApplicationRepository(MasterDbContext db) => _db = db;

    public Task<List<Application>> GetByCitizenAsync(Guid citizenId) =>
        _db.Applications.Include(a => a.Status).Where(a => a.CitizenId == citizenId).ToListAsync();

    public Task<Application?> GetByIdAsync(Guid id) =>
        _db.Applications.Include(a => a.Status).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Guid> CreateAsync(Application application)
    {
        _db.Applications.Add(application);
        await _db.SaveChangesAsync();
        return application.Id;
    }
}
