using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.Entities;
using FluentAssertions.Execution;

namespace N1coLoyalty.Application.FunctionalTests.Helpers;

using static Testing;
internal static class UserWalletBalanceHelpers
{
    internal static async Task CreateUserWalletBalanceMock(Transaction transaction, User user, WalletActionValue action)
    {
        var userWalletBalance = new UserWalletBalance
        {
            User = user,
            UserId = user.Id,
            Reason = "Reason",
            Action = action,
            Amount = 500,
            Reference = "ddb43001-89ba-4e6f-b0bc-c74474a6c23e",
            TransactionId = transaction.Id,
            Transaction = transaction
        };
        await AttachEntity(userWalletBalance);
    }
}
