using Microsoft.EntityFrameworkCore;
using SMC.Master.Domain.Entities;
using SMC.Master.Infrastructure.Data;

namespace SMC.Master.Infrastructure.Repositories;

public class TransactionRepository
{
    private readonly MasterDbContext _db;
    public TransactionRepository(MasterDbContext db) => _db = db;

    public Task<List<Transaction>> GetByApplicationAsync(Guid applicationId) =>
        _db.Transactions.Where(t => t.ApplicationId == applicationId).ToListAsync();
}
