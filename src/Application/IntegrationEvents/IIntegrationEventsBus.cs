namespace N1coLoyalty.Application.IntegrationEvents;

public interface IIntegrationEventsBus
{
    Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event)
        where TIntegrationEvent : class;
}