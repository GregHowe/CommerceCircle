using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyEffectDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public EffectTypeValue Type { get; set; }
    public string Status { get; set; } = string.Empty;
    public required LoyaltyEffectActionDto Action { get; set; }
    public required string CampaignId { get; set; }
    public LoyaltyNotificationDto? Notification { get; set; }
    public LoyaltyRewardDto? Reward { get; set; }
    public LoyaltyProfileChallengeDto? Challenge { get; set; }
}
