namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyTier
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public int PointThreshold { get; set; }
    public int? PointsToNextTier { get; set; }
    public List<ProfileChallenge> Challenges { get; set; } = [];
    public List<LoyaltyBenefit> Benefits { get; set; } = [];
    public required bool IsCurrent { get; set; }
    public required bool IsLocked { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
