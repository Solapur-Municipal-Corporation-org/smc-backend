namespace SMC.Master.Application.DTOs.Transaction;

public record TransactionDto(
    Guid Id,
    Guid ApplicationId,
    decimal Amount,
    string Mode,
    string Status,
    DateTime TransactionDate,
    string ReferenceNumber
);
