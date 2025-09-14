using BusinessEvents.Contracts.Issuing;
using BusinessEvents.Contracts.Issuing.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Services.Void;
using N1coLoyalty.Application.Consumers.Common;
using N1coLoyalty.Application.Consumers.Transaction.Common;
using N1coLoyalty.Application.Common.Utils;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Consumers.Transaction;

public class TransactionStatusChangeConsumer(
    ILogger<TransactionStatusChangeConsumer> logger,
    IApplicationDbContext dbContext,
    ILoyaltyEngine loyaltyEngine,
    VoidService voidService,
    IConfiguration configuration) : IConsumer<TransactionStatusChange>
{
    private static readonly HashSet<string> ValidTransactionTypes =
    [
        TransactionTypeConstant.CashIn,
        TransactionTypeConstant.OutgoingTransfer,
        TransactionTypeConstant.Expense,
        TransactionTypeConstant.Reversal
    ];

    private static readonly HashSet<string> ValidTransactionStatuses =
    [
        TransactionStatusConstant.Completed,
        TransactionStatusConstant.Reverted
    ];

    public async Task Consume(ConsumeContext<TransactionStatusChange> context)
    {
        try
        {
            logger.LogInformation("New message in {@Consumer} {@Message}", GetType().Name, context.Message);
            var message = context.Message;

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ExternalUserId == message.User.ExternalUserId);
            if (user is null)
            {
                logger.LogWarning(
                    "Transaction Status Change Consumer: User not found, ExternalUserId: {@ExternalUserId}, TransactionId: {@Id}",
                    message.User.ExternalUserId, message.Id);
                return;
            }

            if (!ValidTransactionTypes.Contains(message.TransactionType))
            {
                logger.LogWarning(
                    "Transaction Status Change Consumer: Invalid transaction type {@TransactionType}, TransactionId: {@Id}",
                    message.TransactionType, message.Id);
                return;
            }

            if (!ValidTransactionStatuses.Contains(message.TransactionStatus))
            {
                logger.LogWarning(
                    "Transaction Status Change Consumer: Invalid transaction status {@TransactionStatus}, TransactionId: {@Id}",
                    message.TransactionStatus, message.Id);
                return;
            }

            var eventType = GetEventType(message);

            var profile = new LoyaltyProfileDto
            {
                IntegrationId = user.ExternalUserId, FirstName = user.Name, PhoneNumber = user.Phone,
            };

            var attributes = GetBaseAttributes(message);

            if (eventType == EventTypeConstant.Expense)
            {
                if (message.PosMetadata is null)
                {
                    logger.LogWarning(
                        "Transaction Status Change Consumer: POS metadata not found, TransactionId: {@Id}",
                        message.Id);
                }
                else
                {
                    AddExpenseMetadataAttributes(message.PosMetadata, attributes);
                }
            }


            var loyaltyProgramIntegrationId = configuration["LoyaltyCore:LoyaltyProgramIntegrationId"] ??
                                              string.Empty;

            if (message.TransactionStatus == TransactionStatusConstant.Completed)
            {
                await loyaltyEngine.ProcessEventAsync(new ProcessEventInputDto
                {
                    LoyaltyProgramIntegrationId = loyaltyProgramIntegrationId,
                    EventType = eventType,
                    LoyaltyProfile = profile,
                    Attributes = attributes
                });
            }

            if (message.TransactionStatus == TransactionStatusConstant.Reverted)
            {
                var transaction =
                    await dbContext.Transactions.FirstOrDefaultAsync(t => t.IntegrationId == message.Id.ToString());

                if (transaction is null)
                {
                    logger.LogWarning(
                        "Transaction Status Change Consumer: Transaction to reverse not found, TransactionId: {@Id}",
                        message.Id);
                    return;
                }

                await voidService.ProcessVoid(transaction.Id, TransactionDescription.Challenge,
                    TransactionOriginValue.Challenge, true, CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Transaction Status Change Consumer: Error processing message. Error: {@Error} Message: {@Message}",
                e.Message, context.Message);
        }
    }

    private static void AddExpenseMetadataAttributes(PosMetadata posMetadata, Dictionary<string, object> attributes)
    {
        var metadataAttributes = new Dictionary<string, object?>
        {
            { AttributesConstants.MerchantId, posMetadata.MerchantId },
            { AttributesConstants.Mcc, posMetadata.Mcc },
            { AttributesConstants.TerminalId, posMetadata.TerminalId }
        };

        foreach ((string key, object? value) in metadataAttributes)
        {
            if (value is not null)
                attributes.TryAdd(key, value);
        }
    }

    private static Dictionary<string, object> GetBaseAttributes(TransactionStatusChange message)
    {
        return new Dictionary<string, object>
        {
            { AttributesConstants.TransactionId, message.Id },
            { AttributesConstants.TransactionAmount, StringUtils.RoundToDown(message.Amount) }
        };
    }

    private static string GetEventType(TransactionStatusChange message)
    {
        return message.TransactionType switch
        {
            TransactionTypeConstant.CashIn => EventTypeConstant.CashIn,
            TransactionTypeConstant.OutgoingTransfer => EventTypeConstant.OutgoingTransfer,
            TransactionTypeConstant.Expense => EventTypeConstant.Expense,
            _ => throw new ArgumentOutOfRangeException(message.TransactionType, "Invalid transaction type")
        };
    }
}
