using N1coLoyalty.Application.IssuingEvents;

namespace N1coLoyalty.Infrastructure.IssuingEvents;

public class IssuingEventsBus(IIssuingBus issuingBus): IIssuingEventsBus
{
    public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event) where TIntegrationEvent : class
    {
        await issuingBus.Publish(@event);
    }
}
