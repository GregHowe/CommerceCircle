namespace N1coLoyalty.Application.IssuingEvents;

public interface IIssuingEventsBus
{
    public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event)
        where TIntegrationEvent : class;
}
