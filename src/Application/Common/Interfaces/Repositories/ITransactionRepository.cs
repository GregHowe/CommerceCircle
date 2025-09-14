using N1coLoyalty.Application.Common.Filters;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Interfaces.Repositories;

public interface ITransactionRepository
{
    IQueryable<TransactionDto> GetTransactionsByUser(Guid? userId);
    Task<int> GetTransactionsCountByFrequency(User user, FrequencyValue userEventFrequency, TransactionCountFilter filter);
    Task<Transaction> CreateTransaction(Transaction transaction, CancellationToken cancellationToken);
    Task<Transaction?> GetOldestUnredeemedRetryTransaction(Guid userId, CancellationToken cancellationToken);
    Task<Transaction> UpdateTransactionStatus(Transaction transaction, TransactionStatusValue status, CancellationToken cancellationToken);
    Task UpdateTransaction(Transaction transaction, CancellationToken cancellationToken);
    IQueryable<Transaction> GetQueryableTransactions();
    Task<Transaction> GetTransactionById(Guid id);
}
