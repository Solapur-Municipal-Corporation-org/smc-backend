using Microsoft.EntityFrameworkCore;
using SMC.Master.Domain.Entities;
using SMC.Master.Infrastructure.Data;

namespace SMC.Master.Infrastructure.Repositories;

public class ServiceRepository
{
    private readonly MasterDbContext _db;
    public ServiceRepository(MasterDbContext db) => _db = db;

    public Task<List<Service>> GetAllAsync() => _db.Services.ToListAsync();

    public Task<List<Service>> GetByDepartmentAsync(Guid departmentId) =>
        _db.DepartmentServices
            .Where(ds => ds.DepartmentId == departmentId)
            .Select(ds => ds.Service)
            .ToListAsync();
}
