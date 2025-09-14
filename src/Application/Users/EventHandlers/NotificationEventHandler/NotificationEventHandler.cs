using Functions;
using N1coLoyalty.Application.NotificationEvents;
using N1coLoyalty.Domain.Events;

namespace N1coLoyalty.Application.Users.EventHandlers.NotificationEventHandler;

public class NotificationEventHandler(INotificationEventBus notificationEventBus): INotificationHandler<NotificationEvent>
{
    public async Task Handle(NotificationEvent notificationEvent, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            SubscriberExternalId = notificationEvent.SubscriberExternalId,
            Title = notificationEvent.Title,
            Text = notificationEvent.Text,
            FormattedText = notificationEvent.FormattedText,
            Type = notificationEvent.Type,
            ObjectId = notificationEvent.ObjectId,
            Origin = "loyalty-n1co",
            RequestId = Guid.NewGuid().ToString()
        };
        
        await notificationEventBus.PublishAsync(notification);
    }
}
