using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.FunctionalTests.Helpers;

using static Testing;
internal static class TermsConditionsHelpers
{
    internal static async Task<TermsConditionsInfo> CreateTermsConditions()
    {
        var user = new User() { ExternalUserId = "anyIdUser", Id = Guid.NewGuid() };
        var termsConditions = new TermsConditionsInfo { Url = "test1.com", IsCurrent = true, Version = "1.0.0" };
        
        var termsConditionsAccepted = new TermsConditionsAcceptance
        {
            User = user,
            UserId = user.Id,
            TermsConditionsId = termsConditions.Id,
            TermsConditionsInfo = termsConditions,
            IsAccepted = true
        };
        await AddAsync(termsConditionsAccepted);

        return termsConditions;
    }
}
