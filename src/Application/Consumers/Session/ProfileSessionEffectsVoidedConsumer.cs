using BusinessEvents.Contracts.Loyalty;
using MassTransit;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Application.Common.Utils;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Consumers.Session;

public class ProfileSessionEffectsVoidedConsumer(
    ILogger<ProfileSessionEffectsVoidedConsumer> logger,
    ILoyaltyEventService loyaltyEventService,
    IApplicationDbContext dbContext) : IConsumer<ProfileSessionEffectsVoided>
{
    public async Task Consume(ConsumeContext<ProfileSessionEffectsVoided> context)
    {
        try
        {
            logger.LogInformation("New message in {@Consumer} {@Message}", GetType().Name, context.Message);

            var message = context.Message;

            if (message.VoidProfileSession.Profile is null)
            {
                logger.LogWarning("Profile Session Effects Voided Consumer: Profile is empty: {@Message}", message);
                return;
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u =>
                u.ExternalUserId == message.VoidProfileSession.Profile.IntegrationId);
            if (user is null)
            {
                logger.LogWarning("Profile Session Effects Voided Consumer: User not found, ExternalUserId: {@ExternalUserId}",
                    message.VoidProfileSession.Profile.IntegrationId);
                return;
            }

            var voidedProfileSessionEvent = message.VoidProfileSession.Event is not null
                ? new LoyaltyEventDto()
                {
                    EventType = message.VoidProfileSession.Event.EventType,
                    Attributes = message.VoidProfileSession.Event.Attributes
                }
                : null;

            var originalProfileSessionId = message.OriginalProfileSession.ProfileSessionId.ToString();
            var voidProfileSessionId = message.VoidProfileSession.ProfileSessionId.ToString();

            var originalTransactions = await dbContext.Transactions
                .Where(t => t.ProfileSessionId == originalProfileSessionId)
                .ToListAsync();
            foreach (var originalTransaction in originalTransactions)
            {
                originalTransaction.TransactionStatus = TransactionStatusValue.Voided;
                originalTransaction.Metadata.Add(TransactionMetadata.VoidProfileSessionId, voidProfileSessionId);
            }

            var effects = message.VoidProfileSession.Effects.Select(EffectUtils.EffectSelector()).ToList();

            var voidTransactions =
                await loyaltyEventService.ProcessEffects(effects, user, voidedProfileSessionEvent,
                    voidProfileSessionId);
            foreach (var transaction in voidTransactions)
                transaction.Metadata.Add(TransactionMetadata.VoidedProfileSessionId, originalProfileSessionId);

            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Profile Session Effects Voided Consumer: Error processing message. Error: {@Error} Message: {@Message}", e.Message,
                context.Message);
        }
    }
}