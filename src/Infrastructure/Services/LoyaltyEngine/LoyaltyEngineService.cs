using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Utils;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore;
using N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

namespace N1coLoyalty.Infrastructure.Services.LoyaltyEngine;

public class LoyaltyEngineService(LoyaltyCoreHttpClient loyaltyCoreHttpClient) : ILoyaltyEngine
{
    public async Task<ProfileCreationDto> GetOrCreateProfile(string loyaltyProgramIntegrationId,
        LoyaltyCreateProfileInput input)
    {
        var profileInput = new LoyaltyProfileInput()
        {
            IntegrationId = input.IntegrationId,
            LoyaltyProgramIntegrationId = loyaltyProgramIntegrationId,
            PhoneNumber = input.PhoneNumber
        };

        var profile = await loyaltyCoreHttpClient.GetOrCreateProfile(loyaltyProgramIntegrationId, profileInput);

        if (profile is null)
        {
            return new ProfileCreationDto { Success = false, Message = "Profile creation Error", };
        }

        return new ProfileCreationDto
        {
            Success = true, Message = "Profile created successfully", Profile = MapProfileDto(profile)
        };
    }

    public async Task<LoyaltyProfileDto?> GetProfile(string integrationId)
    {
        var profileResponse = await loyaltyCoreHttpClient.GetProfile(integrationId);
        return profileResponse is null ? null : MapProfileDto(profileResponse);
    }

    public async Task<ProcessEventResponseDto> ProcessEventAsync(ProcessEventInputDto input)
    {
        var profile = new LoyaltyProfileInput
        {
            IntegrationId = input.LoyaltyProfile.IntegrationId,
            LoyaltyProgramIntegrationId = input.LoyaltyProgramIntegrationId,
            PhoneNumber = input.LoyaltyProfile.PhoneNumber
        };
        var processEventResponse = await loyaltyCoreHttpClient.ProcessEventAsync(input.EventType,
            input.LoyaltyProgramIntegrationId, profile, input.Attributes);

        if (processEventResponse is null)
        {
            return new ProcessEventResponseDto
            {
                Success = false, Message = "Error al procesar evento", Code = "EVENT_PROCESSING_ERROR",
            };
        }

        var processEventResponseDto = new ProcessEventResponseDto
        {
            Code = "OK", Message = "Evento procesado correctamente", Success = true,
        };

        return processEventResponseDto;
    }

    public async Task<VoidSessionResponseDto> VoidSession(VoidSessionInputDto input)
    {
        var response = await loyaltyCoreHttpClient.VoidProfileSessionEffects(input.ProfileSessionId);
        if (response?.Data?.VoidProfileSession.Effects is null)
        {
            return new VoidSessionResponseDto
            {
                Success = false, Message = "Error al obtener la reversa de la sesión", Code = "SESSION_VOID_ERROR",
            };
        }

        var responseDto = new VoidSessionResponseDto()
        {
            Code = response.Code,
            Message = response.Message,
            Success = response.Success,
            Data = new VoidSessionDataResponseDto()
            {
                OriginalProfileSession = MapLoyaltyProfileSessionDto(response.Data.OriginalProfileSession),
                VoidProfileSession = MapLoyaltyProfileSessionDto(response.Data.VoidProfileSession)
            }
        };

        return responseDto;
    }

    public async Task<VoidSessionResponseDto> VoidSessionAsync(VoidSessionInputDto input)
    {
        var voidProfileSessionEffectsAsyncResponse =
            await loyaltyCoreHttpClient.VoidProfileSessionEffectsAsync(input.ProfileSessionId);

        if (voidProfileSessionEffectsAsyncResponse is null)
        {
            return new VoidSessionResponseDto
            {
                Success = false, Message = "Error al obtener la reversa de la sesión", Code = "SESSION_VOID_ERROR",
            };
        }

        var responseDto = new VoidSessionResponseDto
        {
            Code = "OK", Message = "Proceso de reversa de sesión iniciado correctamente", Success = true,
        };

        return responseDto;
    }

    public async Task<RewardByProbabilityResponseDto> GetRewardByProbability(RewardByProbabilityInputDto input)
    {
        var profile = new LoyaltyProfileInput()
        {
            IntegrationId = input.LoyaltyProfile.IntegrationId,
            LoyaltyProgramIntegrationId = input.LoyaltyProgramIntegrationId,
            PhoneNumber = input.LoyaltyProfile.PhoneNumber
        };
        var response = await loyaltyCoreHttpClient.GetRewardByProbability(input.CampaignIntegrationId,
            input.LoyaltyProgramIntegrationId, profile);
        if (response?.Effect is null)
        {
            return new RewardByProbabilityResponseDto
            {
                Success = false, Message = "Error al obtener recompensa", Code = "REWARD_ERROR",
            };
        }

        var rewardByProbabilityResponseDto = new RewardByProbabilityResponseDto()
        {
            Code = response.Code,
            Message = response.Message,
            Success = response.Success,
            Effect = MapLoyaltyEffectDto(response.Effect)
        };

        return rewardByProbabilityResponseDto;
    }

    public async Task<LoyaltyCampaignDto?> GetCampaign(
        string integrationId,
        string? profileIntegrationId = null,
        string? loyaltyProgramIntegrationId = null,
        bool? includeRewards = false)
    {
        // TODO: Add ExtraAttemptCost in Loyalty Core Campaign Entity
        var campaignResponse = await loyaltyCoreHttpClient.GetCampaign(integrationId, profileIntegrationId,
            loyaltyProgramIntegrationId, includeRewards);
        return campaignResponse is null
            ? null
            : new LoyaltyCampaignDto
            {
                Id = campaignResponse.Id,
                Name = campaignResponse.Name,
                TotalBudget = campaignResponse.TotalBudget,
                ConsumedBudget = campaignResponse.ConsumedBudget,
                WalletConversionRate = campaignResponse.WalletConversionRate,
                UserEventFrequency = campaignResponse.UserEventFrequency,
                UserEventFrequencyLimit = campaignResponse.UserEventFrequencyLimit,
                EventCost = campaignResponse.EventCost,
                ExtraAttemptCost = campaignResponse.ExtraAttemptCost,
                Rewards = campaignResponse.Rewards?.Select(r => new LoyaltyRewardDto
                    {
                        Id = r.Id, Name = r.Name, IntegrationId = r.IntegrationId
                    })
                    .ToList() ?? new List<LoyaltyRewardDto>()
            };
    }

    private static LoyaltyProfileDto MapProfileDto(LoyaltyProfile loyaltyProfileResponse)
    {
        return new LoyaltyProfileDto
        {
            IntegrationId = loyaltyProfileResponse.IntegrationId,
            FirstName = loyaltyProfileResponse.FirstName,
            LastName = loyaltyProfileResponse.LastName,
            Email = loyaltyProfileResponse.Email,
            PhoneNumber = loyaltyProfileResponse.PhoneNumber,
            LoyaltyPrograms = loyaltyProfileResponse.LoyaltyPrograms.Select(lp => new LoyaltyProgramDto
                {
                    Id = lp.Id.ToString(),
                    IntegrationId = lp.IntegrationId,
                    Name = lp.Name,
                    Description = lp.Description,
                    Tiers = lp.Tiers.Select(t => new LoyaltyTierDto
                        {
                            Id = t.Id,
                            Name = t.Name,
                            IsCurrent = t.IsCurrent,
                            PointThreshold = Convert.ToInt32(t.PointThreshold),
                            PointsToNextTier =
                                t.PointsToNextTier != null ? Convert.ToInt32(t.PointsToNextTier) : null,
                            MotivationalMessage = LoyaltyEngineUtils.GetMotivationalMessageFromMetadata(t.Metadata),
                            IsLocked = t.IsLocked,
                            Challenges = t.Challenges.Select(MapLoyaltyProfileChallengeDto).ToList(),
                            Benefits = t.Benefits.Select(MapLoyaltyBenefitDto).ToList()
                        })
                        .ToList()
                })
                .ToList(),
            Referral = MapReferralDto(loyaltyProfileResponse.Referral),
            Balance = new WalletBalanceResponseDto()
            {
                Credit = loyaltyProfileResponse.Balance?.Credit ?? 0,
                Debit = loyaltyProfileResponse.Balance?.Debit ?? 0,
                HistoricalCredit = loyaltyProfileResponse.Balance?.HistoricalCredit ?? 0,
            }
        };
    }

    private static LoyaltyBenefitDto MapLoyaltyBenefitDto(LoyaltyBenefit benefit)
    {
        return new LoyaltyBenefitDto
        {
            Id = benefit.Id,
            Description = benefit.Description,
            Type = LoyaltyEngineUtils.GetBenefitTypeFromIntegrationId(benefit.IntegrationId),
        };
    }

    private static LoyaltyReferralDto? MapReferralDto(LoyaltyReferral? referral)
    {
        return referral is null
            ? null
            : new LoyaltyReferralDto() { Code = referral.Code, IsActive = referral.IsActive };
    }

    private static LoyaltyEffectDto MapLoyaltyEffectDto(LoyaltyEffect loyaltyEffect)
    {
        return new LoyaltyEffectDto()
        {
            Id = loyaltyEffect.Id,
            Name = loyaltyEffect.Name,
            Type =
                Enum.TryParse(loyaltyEffect.Type, out EffectTypeValue effectTypeValue)
                    ? effectTypeValue
                    : EffectTypeValue.Unknown,
            Status = loyaltyEffect.Status,
            CampaignId = loyaltyEffect.CampaignId,
            Action = new LoyaltyEffectActionDto
            {
                Type = loyaltyEffect.Action.Type,
                Amount = loyaltyEffect.Action.Amount,
                Metadata = loyaltyEffect.Action.Metadata,
            },
            Notification = loyaltyEffect.Notification != null
                ? new LoyaltyNotificationDto
                {
                    Title = loyaltyEffect.Notification.Title,
                    Message = loyaltyEffect.Notification.Message,
                    FormattedMessage = loyaltyEffect.Notification.FormattedMessage,
                    Type = loyaltyEffect.Notification.Type
                }
                : null,
            Reward = loyaltyEffect.Reward != null
                ? new LoyaltyRewardDto
                {
                    Id = loyaltyEffect.Reward.Id,
                    Name = loyaltyEffect.Reward.Name,
                    IntegrationId = loyaltyEffect.Reward.IntegrationId,
                    EffectSubType =
                        LoyaltyEngineUtils.GetEffectSubTypeFromRewardIntegrationId(loyaltyEffect.Reward
                            .IntegrationId)
                }
                : null,
            Challenge = loyaltyEffect.Challenge != null
                ? new LoyaltyProfileChallengeDto
                {
                    Id = loyaltyEffect.Challenge.Id,
                    Name = loyaltyEffect.Challenge.Name,
                    Description = loyaltyEffect.Challenge.Description,
                    Target = loyaltyEffect.Challenge.Target,
                    EffectValue = loyaltyEffect.Challenge.EffectActionValue,
                    Type = LoyaltyEngineUtils.GetChallengeTypeFromMetadata(loyaltyEffect.Challenge.Metadata),
                    EffectType = LoyaltyEngineUtils.GetEffectTypeFromMetadata(loyaltyEffect.Challenge.Metadata),
                    EffectSubType = LoyaltyEngineUtils.GetEffectSubTypeFromMetadata(loyaltyEffect.Challenge.Metadata),
                }
                : null,
        };
    }

    private static LoyaltyEventDto MapLoyaltyEventDto(LoyaltyEvent loyaltyEvent)
    {
        return new LoyaltyEventDto() { EventType = loyaltyEvent.EventType, Attributes = loyaltyEvent.Attributes, };
    }

    private static LoyaltyProfileChallengeDto MapLoyaltyProfileChallengeDto(ProfileChallenge loyaltyProfileChallenge)
    {
        return new LoyaltyProfileChallengeDto
        {
            Id = loyaltyProfileChallenge.Id,
            Name = loyaltyProfileChallenge.Name,
            Description = loyaltyProfileChallenge.Description,
            Target = loyaltyProfileChallenge.Target,
            TargetProgress = loyaltyProfileChallenge.TargetProgress,
            EffectValue = loyaltyProfileChallenge.EffectActionValue,
            Type = LoyaltyEngineUtils.GetChallengeTypeFromMetadata(loyaltyProfileChallenge.Metadata),
            EffectType = LoyaltyEngineUtils.GetEffectTypeFromMetadata(loyaltyProfileChallenge.Metadata),
            EffectSubType = LoyaltyEngineUtils.GetEffectSubTypeFromMetadata(loyaltyProfileChallenge.Metadata),
            Stores = loyaltyProfileChallenge.Stores.Select(s => new LoyaltyStoreDto
            {
                Id = s.Id, Name = s.Name, Description = s.Description, ImageUrl = s.ImageUrl,
            }).ToList(),
        };
    }

    private static LoyaltyProfileSessionDto MapLoyaltyProfileSessionDto(LoyaltyProfileSession loyaltyProfileSession)
    {
        return new LoyaltyProfileSessionDto
        {
            Id = loyaltyProfileSession.Id,
            Status = loyaltyProfileSession.Status,
            Effects = loyaltyProfileSession.Effects.Select(MapLoyaltyEffectDto).ToList(),
            Profile = new LoyaltyProfileBaseDto()
            {
                IntegrationId = loyaltyProfileSession.Profile.IntegrationId,
                PhoneNumber = loyaltyProfileSession.Profile.PhoneNumber,
                Email = loyaltyProfileSession.Profile.Email,
                FirstName = loyaltyProfileSession.Profile.FirstName,
                LastName = loyaltyProfileSession.Profile.LastName,
            },
            Event = MapLoyaltyEventDto(loyaltyProfileSession.Event)
        };
    }
}
