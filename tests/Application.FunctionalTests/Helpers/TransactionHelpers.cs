using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.FunctionalTests.Helpers;

using static Testing;
internal static class TransactionHelpers
{
    internal static async Task<Transaction> CreateTransactionMock(User user, decimal amount )
    {
        var transaction = new Transaction
        {
            User = user,
            UserId = user.Id,
            Name = "Name",
            Description = "Description",
            TransactionStatus = TransactionStatusValue.Created,
            TransactionType = EffectTypeValue.Credit,
            TransactionSubType = EffectSubTypeValue.Point,
            IntegrationId = Guid.NewGuid().ToString(),
            Amount = amount,
            TransactionOrigin = TransactionOriginValue.Admin
        };
        await AttachEntity(transaction);
        return transaction;
    }
    
    internal static async Task<Transaction> CreateTransactionWithSessionIdMock(User user, decimal amount )
    {
        var transaction = new Transaction
        {
            User = user,
            UserId = user.Id,
            Name = TransactionName.PointsReward,
            Description = TransactionDescription.Roulette,
            TransactionStatus = TransactionStatusValue.Created,
            TransactionType = EffectTypeValue.Credit,
            TransactionSubType = EffectSubTypeValue.Point,
            IntegrationId = Guid.NewGuid().ToString(),
            ProfileSessionId = Guid.NewGuid().ToString(),
            Amount = amount,
            TransactionOrigin = TransactionOriginValue.Game
        };
        await AttachEntity(transaction);
        return transaction;
    }
}
