namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyAction
{
    public required string Type { get; set; }
    public decimal Amount { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
