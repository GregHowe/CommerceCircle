using BusinessEvents.Contracts.Issuing;
using BusinessEvents.Contracts.Issuing.Enums;
using BusinessEvents.Contracts.Issuing.Models;
using N1coLoyalty.Application.IssuingEvents;
using N1coLoyalty.Domain.Events;

namespace N1coLoyalty.Application.Users.EventHandlers.ChallengeCompletedEventHandler;

public class ChallengeCompletedEventHandler(IIssuingEventsBus issuingEventsBus):INotificationHandler<ChallengeCompletedEvent>
{
    public async Task Handle(ChallengeCompletedEvent notification, CancellationToken cancellationToken)
    {
        var issuingEvent = new ChallengeCompleted()
        {
            User = new User()
            {
                ExternalUserId = notification.User.ExternalUserId,
                Name = notification.User.Name ?? string.Empty,
                PhoneNumber = notification.User.Phone ?? string.Empty,
            },
            Effect = new Effect()
            {
                Amount = notification.Transaction.Amount,
                ActionType = EffectActionType.AddPoints
            },
            Event = new Event()
            {
                EventType = notification.EventType,
                Attributes = notification.Attributes
            }
        };
        
        await issuingEventsBus.PublishAsync(issuingEvent);
    }
}
