namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyReward
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public required string IntegrationId { get; set; }
}
