namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyChallenge
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? Target { get; set; }
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public decimal? EffectActionValue { get; set; }
}
