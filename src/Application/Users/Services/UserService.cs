using N1coLoyalty.Application.Common.Filters;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.ValueObjects;

namespace N1coLoyalty.Application.Users.Services;

public class UserService(ITransactionRepository transactionRepository, IApplicationDbContext context)
{
    public async Task<int?> GetRemainingAttempts(User user, LoyaltyCampaignDto? campaign)
    {
        if (campaign?.UserEventFrequency is null || campaign.UserEventFrequencyLimit is null) return null;
        
        var userEventFrequency = EventFrequency.For(campaign.UserEventFrequency).Frequency;
        var userEventFrequencyLimit = campaign.UserEventFrequencyLimit;

        var transactionsCountByFrequency =
            await transactionRepository.GetTransactionsCountByFrequency(user, userEventFrequency,
                new TransactionCountFilter
                {
                    TransactionType = EffectTypeValue.Reward,
                    TransactionOrigin = TransactionOriginValue.Game,
                });

        return Math.Max(userEventFrequencyLimit.Value - transactionsCountByFrequency, 0);
    }
    
    public async Task<bool> OnboardingCompleted(string externalId, CancellationToken cancellationToken) =>
        await context.Users.AnyAsync(u => u.ExternalUserId == externalId && u.OnboardingCompleted,
            cancellationToken: cancellationToken);
}
