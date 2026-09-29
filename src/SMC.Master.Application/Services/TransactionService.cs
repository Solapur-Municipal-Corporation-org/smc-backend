using SMC.Master.Application.DTOs.Transaction;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.Application.Services;

public class TransactionService : ITransactionService
{
    public Task<List<TransactionDto>> GetByApplicationAsync(Guid applicationId) => throw new NotImplementedException();
}
