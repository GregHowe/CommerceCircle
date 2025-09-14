using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.ValueObjects;

namespace N1coLoyalty.Application.Common.Utils;

public static class LoyaltyEngineUtils
{
    public static ChallengeTypeValue GetChallengeTypeFromMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata != null && metadata.TryGetValue("type", out var typeValue))
        {
            return Enum.TryParse(typeValue, out ChallengeTypeValue challengeTypeValue)
                ? challengeTypeValue
                : ChallengeTypeValue.Unknown;
        }

        return ChallengeTypeValue.Unknown;
    }

    public static EffectTypeValue GetEffectTypeFromMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata != null && metadata.TryGetValue("effectType", out var effectType))
        {
            return Enum.TryParse(effectType, out EffectTypeValue challengeEffectTypeValue)
                ? challengeEffectTypeValue
                : EffectTypeValue.Unknown;
        }

        return EffectTypeValue.Unknown;
    }

    public static EffectSubTypeValue GetEffectSubTypeFromMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata != null && metadata.TryGetValue("effectSubType", out var effectSubType))
        {
            return Enum.TryParse(effectSubType, out EffectSubTypeValue effectSubTypeValue)
                ? effectSubTypeValue
                : EffectSubTypeValue.Unknown;
        }

        return EffectSubTypeValue.Unknown;
    }
    
    public static string? GetMotivationalMessageFromMetadata(IDictionary<string, string>? metadata)
    {
        return metadata != null &&
               metadata.TryGetValue("motivationalMessage", out string? value)
            ? value
            : null;
    }
    
    public static BenefitTypeValue GetBenefitTypeFromIntegrationId(string integrationId) =>
        BenefitType.For(integrationId).Type;

    public static EffectSubTypeValue GetEffectSubTypeFromRewardIntegrationId(string integrationId) =>
        TransactionSubType.For(integrationId).SubType;
}
