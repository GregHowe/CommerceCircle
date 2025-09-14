namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyProgram
{
    public required string Id { get; set; }
    public required string IntegrationId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<LoyaltyTier> Tiers { get; set; } = [];
}
