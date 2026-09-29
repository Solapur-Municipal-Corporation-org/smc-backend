using SMC.Master.Application.DTOs.Transaction;

namespace SMC.Master.Application.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionDto>> GetByApplicationAsync(Guid applicationId);
}
