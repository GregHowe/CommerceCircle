using BusinessEvents.Contracts.Loyalty;
using MassTransit;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Application.Common.Utils;

namespace N1coLoyalty.Application.Consumers.Event;

public class EventProcessedConsumer(
    ILogger<EventProcessedConsumer> logger,
    ILoyaltyEventService loyaltyEventService,
    IApplicationDbContext dbContext
    ) : IConsumer<EventProcessed>
{
    public async Task Consume(ConsumeContext<EventProcessed> context)
    {
        try
        {
            logger.LogInformation("New message in {@Consumer} {@Message}", GetType().Name, context.Message);
            var message = context.Message;
            
            if (message.Profile is null)
            {
                logger.LogWarning(
                    "Event Processed Consumer: Profile is empty: {@Message}", message);
                return;
            }
            
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ExternalUserId == message.Profile.IntegrationId);
            if (user is null)
            {
                logger.LogWarning(
                    "Event Processed Consumer: User not found, ExternalUserId: {@ExternalUserId}", message.Profile.IntegrationId);
                return;
            }

            var processedEvent = message.Event is not null ? new LoyaltyEventDto()
            {
                EventType = message.Event.EventType, Attributes = message.Event.Attributes
            } : null;
            
            var profileSessionId = message.ProfileSessionId.ToString();

            var effects = message.Effects.Select(EffectUtils.EffectSelector())
                .ToList();
            
            await loyaltyEventService.ProcessEffects(effects, user, processedEvent, profileSessionId);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Event Processed Consumer: Error processing message. Error: {@Error} Message: {@Message}",
                e.Message, context.Message);
        }
    }
}
