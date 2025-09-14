namespace Functions;

public class Notification
{
    public required string SubscriberExternalId { get; set; }
    public required string Type { get; set; }
    public required string Title { get; set; }
    public required string Text { get; set; }
    public required string FormattedText { get; set; }
    public string? ObjectId { get; set; }
    public required string Origin { get; set; }
    public required string RequestId { get; set; }
}
