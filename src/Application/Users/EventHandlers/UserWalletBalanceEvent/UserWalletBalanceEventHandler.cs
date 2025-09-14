using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.Users.EventHandlers.UserWalletBalanceEvent;

public class UserWalletBalanceEventHandler(IApplicationDbContext context)
    : INotificationHandler<Domain.Events.UserWalletBalanceEvent>
{
    public async Task Handle(Domain.Events.UserWalletBalanceEvent notification, CancellationToken cancellationToken)
    {
        var userWalletBalance = new UserWalletBalance
        {
            UserId = notification.UserId,
            Amount = notification.Amount,
            Reason = notification.Reason,
            Action = notification.Action,
            Reference = notification.Reference,
            TransactionId = notification.TransactionId
        };

        await context.UserWalletBalances.AddAsync(userWalletBalance, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
