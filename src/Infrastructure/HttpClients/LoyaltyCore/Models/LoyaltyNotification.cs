namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyNotification
{
    public required string Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required string FormattedMessage { get; set; }
}
