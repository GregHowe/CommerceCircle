namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyEvent
{
    public string? EventType { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
}
