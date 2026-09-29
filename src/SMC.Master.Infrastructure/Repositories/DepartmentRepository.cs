using Microsoft.EntityFrameworkCore;
using SMC.Master.Domain.Entities;
using SMC.Master.Infrastructure.Data;

namespace SMC.Master.Infrastructure.Repositories;

public class DepartmentRepository
{
    private readonly MasterDbContext _db;
    public DepartmentRepository(MasterDbContext db) => _db = db;

    public Task<List<Department>> GetAllAsync() => _db.Departments.ToListAsync();
    public Task<Department?> GetByIdAsync(Guid id) => _db.Departments.FirstOrDefaultAsync(d => d.Id == id);
}
