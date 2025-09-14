using N1coLoyalty.Application.NotificationEvents;

namespace N1coLoyalty.Infrastructure.NotificationEvents;

public class NotificationEventsBus(INotificationBus notificationBus): INotificationEventBus
{
    public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event) where TIntegrationEvent : class
    {
        var sendEndpoint = await notificationBus.GetSendEndpoint(new Uri("queue:notifications"));
        await sendEndpoint.Send(@event);
    }
}
