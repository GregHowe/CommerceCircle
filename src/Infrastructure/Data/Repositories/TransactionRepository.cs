using Microsoft.EntityFrameworkCore;
using N1coLoyalty.Application.Common.Filters;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Transactions.Queries.GetTransactions;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Infrastructure.Data.Repositories;

public class TransactionRepository(IApplicationDbContext context, IDateTime dateTimeService) : ITransactionRepository
{
    private const int Offset = -6;

    public IQueryable<TransactionDto> GetTransactionsByUser(Guid? userId)
    {
        var today = dateTimeService.Now.AddHours(Offset).Date;

        return from transaction in context.Transactions
               let created = transaction.Created.AddHours(Offset)
               let operationType = GetTransactionOperationType(transaction)
               where transaction.UserId == userId
               let unredeemedTransaction = transaction.TransactionStatus == TransactionStatusValue.Created
               let todayTransaction = today <= created
               let tag = GetTransactionTag(todayTransaction, unredeemedTransaction)
               let iconCategory = GetIconCategory(transaction, operationType)
               select new TransactionDto
               {
                   Id = transaction.Id,
                   Name = transaction.Name,
                   Description = transaction.Description,
                   Created = created,
                   Type = transaction.TransactionType,
                   SubType = transaction.TransactionSubType,
                   IconCategory = iconCategory,
                   OperationType = operationType,
                   Status = transaction.TransactionStatus,
                   Amount = transaction.Amount,
                   Origin = transaction.TransactionOrigin,
                   Tag = tag,
               };
    }

    private static TransactionTag GetTransactionTag(bool todayTransaction, bool unredeemedTransaction)
    {
        if (unredeemedTransaction) return TransactionTag.Unredeemed;
        return todayTransaction ? TransactionTag.Today : TransactionTag.Previous;
    }

    private static IconCategoryValue GetIconCategory(Transaction transaction, TransactionOperationTypeValue operationType)
    {
        if (transaction is { TransactionSubType: EffectSubTypeValue.Point, TransactionType: EffectTypeValue.Revert })
            return IconCategoryValue.Revert;
        return transaction.TransactionOrigin switch
        {
            TransactionOriginValue.Onboarding => IconCategoryValue.Onboarding,
            TransactionOriginValue.Challenge => MapIconCategoryFromEffectSubType(transaction.TransactionSubType),
            TransactionOriginValue.Game when operationType == TransactionOperationTypeValue.Debit => IconCategoryValue.PointDebit,
            TransactionOriginValue.Shop => IconCategoryValue.Shop,
            TransactionOriginValue.Admin => MapIconCategoryForAdminOperations(transaction.TransactionType, transaction.TransactionSubType),
            _ => MapIconCategoryFromEffectSubType(transaction.TransactionSubType)
        };
    }
    
    private static TransactionOperationTypeValue GetTransactionOperationType(Transaction transaction)
    {
        return transaction.TransactionType switch
        {
            EffectTypeValue.Debit => TransactionOperationTypeValue.Debit,
            EffectTypeValue.Credit => TransactionOperationTypeValue.Credit,
            EffectTypeValue.Revert => TransactionOperationTypeValue.CreditVoid,
            EffectTypeValue.Refund => TransactionOperationTypeValue.DebitVoid,
            _ => TransactionOperationTypeValue.Credit
        };
    }

    private static IconCategoryValue MapIconCategoryFromEffectSubType(EffectSubTypeValue effectSubType)
    {
        return effectSubType switch
        {
            EffectSubTypeValue.Retry => IconCategoryValue.Retry,
            EffectSubTypeValue.Cash => IconCategoryValue.Cash,
            EffectSubTypeValue.Compensation => IconCategoryValue.Compensation,
            EffectSubTypeValue.Point => IconCategoryValue.Point,
            _ => IconCategoryValue.Unknown
        };
    }
    
    private static IconCategoryValue MapIconCategoryForAdminOperations(EffectTypeValue effectTypeValue,EffectSubTypeValue effectSubType)
    {
        return effectTypeValue switch
        {
            EffectTypeValue.Revert => IconCategoryValue.Revert,
            _ => MapIconCategoryFromEffectSubType(effectSubType)
        };
    }

    public async Task<int> GetTransactionsCountByFrequency(User user, FrequencyValue userEventFrequency,
        TransactionCountFilter filter)
    {
        var today = dateTimeService.Now.AddHours(Offset).Date;
        return userEventFrequency switch
        {
            FrequencyValue.Daily => await context.Transactions.CountAsync(t =>
                t.UserId == user.Id && today <= t.Created.AddHours(Offset) &&
                (filter.TransactionType == null || t.TransactionType == filter.TransactionType) &&
                (filter.TransactionOrigin == null || t.TransactionOrigin == filter.TransactionOrigin)),
            FrequencyValue.Monthly => await context.Transactions.CountAsync(t =>
                t.UserId == user.Id && today.Month <= t.Created.AddHours(Offset).Month &&
                (filter.TransactionType == null || t.TransactionType == filter.TransactionType) &&
                (filter.TransactionOrigin == null || t.TransactionOrigin == filter.TransactionOrigin)),
            _ => throw new ArgumentOutOfRangeException(nameof(userEventFrequency), userEventFrequency, null)
        };
    }

    public async Task<Transaction> CreateTransaction(Transaction transaction, CancellationToken cancellationToken)
    {
        await context.Transactions.AddAsync(transaction, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return transaction;
    }

    public async Task<Transaction?> GetOldestUnredeemedRetryTransaction(Guid userId,
        CancellationToken cancellationToken)
    {
        return await context.Transactions
            .Where(t => t.UserId == userId && t.TransactionSubType == EffectSubTypeValue.Retry &&
                        t.TransactionStatus == TransactionStatusValue.Created)
            .OrderBy(t => t.Created)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<Transaction> UpdateTransactionStatus(Transaction transaction, TransactionStatusValue status,
        CancellationToken cancellationToken)
    {
        transaction.TransactionStatus = status;
        context.Transactions.Update(transaction);
        await context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task UpdateTransaction(Transaction transaction, CancellationToken cancellationToken)
    {
        context.Transactions.Update(transaction);
        await context.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<Transaction> GetQueryableTransactions()
    {
        return context.Transactions;
    }

    public async Task<Transaction> GetTransactionById(Guid id)
    {
        return await context.Transactions
            .Include(t => t.User)
            .SingleAsync( x => x.Id == id );
    }
}
