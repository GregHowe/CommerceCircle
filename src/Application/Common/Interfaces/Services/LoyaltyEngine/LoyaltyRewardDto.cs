using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyRewardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public required string IntegrationId { get; set; }
    public EffectSubTypeValue EffectSubType { get; set; }
}
