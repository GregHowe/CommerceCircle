using Microsoft.Extensions.Configuration;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Users.Services;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Game.Queries;

public class GetGameSettingsQuery : IRequest<GameSettingsDto>
{
    public EventTypeValue? EventType { get; set; }
    
    public class GetGameSettingsQueryHandler(
        ILoyaltyEngine loyaltyEngine,
        ITransactionRepository transactionRepository,
        IUser currentUser,
        IUserRepository userRepository,
        UserService userService,
        IConfiguration configuration)
        : IRequestHandler<GetGameSettingsQuery, GameSettingsDto>
    {
        public async Task<GameSettingsDto> Handle(GetGameSettingsQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);
            var userTransactions = transactionRepository.GetTransactionsByUser(user.Id);
            var unredeemedTransactions = await userTransactions
                .Where(ut =>
                    ut.Status == TransactionStatusValue.Created && ut.Type == EffectTypeValue.Reward &&
                    ut.SubType == EffectSubTypeValue.Retry)
                .CountAsync(cancellationToken: cancellationToken);

            request.EventType ??= EventTypeValue.PlayGame;
            var campaignIntegrationId = configuration[$"LoyaltyCore:CampaignIntegrationIds:{request.EventType.ToString()}"] ??
                                        string.Empty;
            var campaign = await loyaltyEngine.GetCampaign(campaignIntegrationId);
            if (campaign?.UserEventFrequency is null || campaign.UserEventFrequencyLimit is null)
            {
                return new GameSettingsDto
                {
                    AttemptCost = campaign is not null ? (int)campaign.EventCost : 0,
                    ExtraAttemptCost = campaign is not null ? (int)campaign.ExtraAttemptCost : 0,
                    UnredeemedFreeAttempts = unredeemedTransactions,
                };
            }

            var remainingAttempts = await userService.GetRemainingAttempts(user, campaign);
            return new GameSettingsDto
            {
                AttemptsLimit = campaign.UserEventFrequencyLimit,
                RemainingAttempts = remainingAttempts,
                AttemptCost = (int)campaign.EventCost,
                UnredeemedFreeAttempts = unredeemedTransactions,
                ExtraAttemptCost = (int)campaign.ExtraAttemptCost 
            };
        }
    }
}
