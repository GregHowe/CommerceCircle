using BusinessEvents.Contracts.Loyalty.Models;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Utils;

public static class EffectUtils
{
    public static Func<Effect, LoyaltyEffectDto> EffectSelector()
    {
        return e =>
        {
            var rewardIntegrationId = e.Reward?.IntegrationId ?? string.Empty;
            return new LoyaltyEffectDto
            {
                Id = e.Id,
                Name = e.Name,
                Type =
                    Enum.TryParse(e.Type.ToString(), out EffectTypeValue type)
                        ? type
                        : EffectTypeValue.Unknown,
                Status = e.Status.ToString(),
                CampaignId = e.CampaignId.ToString(),
                Action =
                    new LoyaltyEffectActionDto
                    {
                        Type = e.Action.Type.ToString(),
                        Amount = e.Action.Amount,
                        Metadata = e.Action.Metadata,
                    },
                Notification =
                    e.Notification is not null
                        ? new LoyaltyNotificationDto
                        {
                            Type = e.Notification.Type,
                            Title = e.Notification.Title,
                            Message = e.Notification.Message,
                            FormattedMessage = e.Notification.FormattedMessage,
                        }
                        : null,
                Reward =
                    e.Reward is not null
                        ? new LoyaltyRewardDto
                        {
                            Id = e.Reward.Id,
                            IntegrationId = rewardIntegrationId,
                            Name = e.Reward.Name,
                            EffectSubType =
                                LoyaltyEngineUtils.GetEffectSubTypeFromRewardIntegrationId(
                                    rewardIntegrationId),
                        }
                        : null,
                Challenge = e.Challenge is not null
                    ? new LoyaltyProfileChallengeDto
                    {
                        Id = e.Challenge.Id.ToString(),
                        Name = e.Challenge.Name,
                        Description = e.Challenge.Description,
                        Target = e.Challenge.Target,
                        EffectValue = e.Challenge.EffectActionValue,
                        Type = LoyaltyEngineUtils.GetChallengeTypeFromMetadata(e.Challenge.Metadata),
                        EffectType = LoyaltyEngineUtils.GetEffectTypeFromMetadata(e.Challenge.Metadata),
                        EffectSubType =
                            LoyaltyEngineUtils.GetEffectSubTypeFromMetadata(e.Challenge.Metadata),
                    }
                    : null,
            };
        };
    }
}