using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

namespace N1coLoyalty.Application.Common.Interfaces;
public interface ILoyaltyEngine
{
    Task<ProfileCreationDto> GetOrCreateProfile(string loyaltyProgramIntegrationId, LoyaltyCreateProfileInput input);
    Task<LoyaltyProfileDto?> GetProfile(string integrationId);
    Task<ProcessEventResponseDto> ProcessEventAsync(ProcessEventInputDto input);
    Task<VoidSessionResponseDto> VoidSession(VoidSessionInputDto input);
    Task<VoidSessionResponseDto> VoidSessionAsync(VoidSessionInputDto input);
    Task<RewardByProbabilityResponseDto> GetRewardByProbability(RewardByProbabilityInputDto input);
    Task<LoyaltyCampaignDto?> GetCampaign(string integrationId, string? profileIntegrationId = null,
        string? loyaltyProgramIntegrationId = null, bool? includeRewards = false);
}
