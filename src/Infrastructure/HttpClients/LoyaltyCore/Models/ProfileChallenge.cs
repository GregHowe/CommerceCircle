namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class ProfileChallenge
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? Target { get; set; }
    public int TargetProgress { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public decimal? EffectActionValue { get; set; }
    public List<Store> Stores { get; set; } = [];
}
