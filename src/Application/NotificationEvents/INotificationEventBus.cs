namespace N1coLoyalty.Application.NotificationEvents;

public interface INotificationEventBus
{
    Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event)
        where TIntegrationEvent : class;
}
