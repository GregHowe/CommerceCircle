namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyProfileSession
{
    public required Guid Id { get; set; }
    public required string Status { get; set; } = string.Empty;
    public required LoyaltyProfileBase Profile { get; set; }
    public required LoyaltyEvent Event { get; set; }
    public required List<LoyaltyEffect> Effects { get; set; } = [];
}
