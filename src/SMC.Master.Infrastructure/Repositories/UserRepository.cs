using Microsoft.EntityFrameworkCore;
using SMC.Master.Domain.Entities;
using SMC.Master.Infrastructure.Data;

namespace SMC.Master.Infrastructure.Repositories;

public class UserRepository
{
    private readonly MasterDbContext _db;
    public UserRepository(MasterDbContext db) => _db = db;

    public Task<User?> GetByUsernameAsync(string username) =>
        _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);
}
