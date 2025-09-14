using BusinessEvents.Contracts.Loyalty.Models;
using MassTransit;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.Events;

namespace N1coLoyalty.Application.Consumers.Wallet;

public class WalletTransactionConsumer(
    ILogger<WalletTransactionConsumer> logger,
    IApplicationDbContext dbContext,
    ITransactionRepository transactionRepository
    ): IConsumer<WalletTransaction>
{
    private const string ShopAccountId = "hugoapp.h4b";

    public async Task Consume(ConsumeContext<WalletTransaction> context)
    {
        try
        {
            logger.LogInformation("New message in {@Consumer} {@Message}", GetType().Name, context.Message);
            var message = context.Message;

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ExternalUserId == message.ProfileIntegrationId);
            if (user is null)
            {
                logger.LogWarning("WalletTransactionConsumer: User not found, ExternalUserId: {@ExternalUserId}",
                    message.ProfileIntegrationId);
                return;
            }

            var validWalletOperations = new List<WalletOperation> { WalletOperation.Debit, WalletOperation.DebitVoid, };
            if (!validWalletOperations.Contains(message.Operation))
            {
                logger.LogWarning("WalletTransactionConsumer: Invalid Operation: {@Operation}", message.Operation);
                return;
            }

            var accountId = message.Metadata.TryGetValue("AccountId", out var originValue) ? originValue : string.Empty;
            var validAccountIds = new List<string> { ShopAccountId };
            if (string.IsNullOrEmpty(accountId) || !validAccountIds.Contains(accountId))
            {
                logger.LogWarning("WalletTransactionConsumer: Invalid AccountId: {@AccountId}", accountId);
                return;
            }

            var (name, description, origin) = accountId switch
            {
                ShopAccountId when message.Operation is WalletOperation.Debit => ("Producto co1ns", "n1co shop",
                    TransactionOriginValue.Shop),
                ShopAccountId when message.Operation is WalletOperation.DebitVoid => ("Devolución de producto co1ns",
                    "n1co shop", TransactionOriginValue.Shop),
                _ => throw new ArgumentOutOfRangeException()
            };

            var transaction = new Domain.Entities.Transaction
            {
                Amount = message.Amount,
                Name = name,
                Description = description,
                TransactionStatus = TransactionStatusValue.Redeemed,
                TransactionType = message.Operation switch
                {
                    WalletOperation.Debit => EffectTypeValue.Debit,
                    WalletOperation.DebitVoid => EffectTypeValue.Refund,
                    _ => EffectTypeValue.Unknown
                },
                TransactionSubType = EffectSubTypeValue.Point,
                UserId = user.Id,
                User = user,
                TransactionOrigin = origin,
            };

            await transactionRepository.CreateTransaction(transaction, CancellationToken.None);

            transaction.User.AddDomainEvent(new UserWalletBalanceEvent
            {
                UserId = user.Id,
                Action = transaction.TransactionType switch
                {
                    EffectTypeValue.Debit => WalletActionValue.Debit,
                    EffectTypeValue.Refund => WalletActionValue.DebitVoid,
                    _ => throw new ArgumentOutOfRangeException()
                },
                Amount = transaction.Amount,
                Reason = transaction.Name,
                Reference = message.Reference,
                TransactionId = transaction.Id,
            });
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Wallet Transaction Consumer: Error processing message. Error: {@Error} Message: {@Message}",
                e.Message, context.Message);
        }
    }
}
