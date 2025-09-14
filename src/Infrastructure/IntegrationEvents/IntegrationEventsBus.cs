using MassTransit;
using N1coLoyalty.Application.IntegrationEvents;

namespace N1coLoyalty.Infrastructure.IntegrationEvents;

public class IntegrationEventsBus(IPublishEndpoint publishEndpoint) : IIntegrationEventsBus
{
    public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event)
        where TIntegrationEvent : class
    {
       await publishEndpoint.Publish(@event);
    }
}
