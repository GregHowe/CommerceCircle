namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyTierDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? MotivationalMessage { get; set; }
    public int PointThreshold { get; set; }
    public int? PointsToNextTier { get; set; }
    public List<LoyaltyProfileChallengeDto> Challenges { get; set; } = [];
    public List<LoyaltyBenefitDto> Benefits { get; set; } = [];
    public required bool IsCurrent { get; set; }
    public required bool IsLocked { get; set; }
}
