using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.Profile.Services;

public class ProfileService(ILoyaltyEngine loyaltyEngine)
{
    public async Task<LoyaltyProfileDto?> GetOrCreateProfile(User user, string loyaltyProgramIntegrationId)
    {
        var profileInput = new LoyaltyCreateProfileInput
        {
            IntegrationId = user.ExternalUserId,
            PhoneNumber = user.Phone,
        };

        var getOrCreateProfileResponse = await loyaltyEngine.GetOrCreateProfile(loyaltyProgramIntegrationId, profileInput);
        return getOrCreateProfileResponse.Success ? getOrCreateProfileResponse.Profile : null;
    }
}