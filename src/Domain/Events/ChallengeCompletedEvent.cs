namespace N1coLoyalty.Domain.Events;

public class ChallengeCompletedEvent: BaseEvent
{
    public required User User { get; set; }
    public required Transaction Transaction { get; set; }
    public string? EventType { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
}
