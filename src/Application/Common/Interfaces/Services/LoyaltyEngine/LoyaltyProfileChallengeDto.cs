using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyProfileChallengeDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ChallengeTypeValue Type { get; set; }
    public int? Target { get; set; }
    public int TargetProgress { get; set; }
    public decimal? EffectValue { get; set; }
    public EffectTypeValue EffectType { get; set; }
    public EffectSubTypeValue EffectSubType { get; set; }
    public List<LoyaltyStoreDto> Stores { get; set; } = [];
}
