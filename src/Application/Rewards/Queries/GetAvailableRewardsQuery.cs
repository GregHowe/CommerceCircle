using Microsoft.Extensions.Configuration;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.ValueObjects;

namespace N1coLoyalty.Application.Rewards.Queries;

public class GetAvailableRewardsQuery: IRequest<List<RewardDto>>
{
    public EventTypeValue? EventType { get; set; }
    public class GetAvailableRewardsQueryHandler(ILoyaltyEngine loyaltyEngine, IConfiguration configuration, IUser currentUser,IUserRepository userRepository)
        : IRequestHandler<GetAvailableRewardsQuery, List<RewardDto>>
    {
        public async Task<List<RewardDto>> Handle(GetAvailableRewardsQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);

            request.EventType ??= EventTypeValue.PlayGame;
            var campaignIntegrationId =
                configuration[$"LoyaltyCore:CampaignIntegrationIds:{request.EventType.ToString()}"] ?? string.Empty;
            var loyaltyProgramIntegrationId = configuration["LoyaltyCore:LoyaltyProgramIntegrationId"] ?? string.Empty;
            var campaign = await loyaltyEngine.GetCampaign(campaignIntegrationId, user.ExternalUserId,
                loyaltyProgramIntegrationId, includeRewards: true);

            return campaign?.Rewards.Select(r => new RewardDto
                {
                    Id = r.Id, Description = r.Name, SubType = TransactionSubType.For(r.IntegrationId).SubType
                })
                .ToList() ?? [];
        }
    }
}
