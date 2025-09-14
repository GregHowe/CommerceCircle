using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Models;

public class TierDto
{
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public int PointThreshold { get; set; }
    public int? PointsToNextTier { get; set; }
    public string? MotivationalMessage { get; set; }
    public LevelStatusValue Status { get; set; }
    public bool IsLocked { get; set; }
    public virtual ICollection<ChallengeDto> Challenges { get; set; } = new List<ChallengeDto>();
    public virtual ICollection<BenefitDto> Benefits { get; set; } = new List<BenefitDto>();
}
