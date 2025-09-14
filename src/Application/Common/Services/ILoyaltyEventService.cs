using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.Common.Services
{
    public interface ILoyaltyEventService
    {
        public Task<Transaction?> ProcessEffect(LoyaltyEffectDto effect, User user);
        public Task<List<Transaction>> ProcessEffects(List<LoyaltyEffectDto> effects, User user, LoyaltyEventDto? processedEvent = null, string? profileSessionId = null);
    }
}
