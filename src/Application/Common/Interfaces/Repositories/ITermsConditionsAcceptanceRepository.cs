using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.Common.Interfaces.Repositories;

public interface ITermsConditionsAcceptanceRepository
{
    Task<TermsConditionsAcceptance> SaveTermsConditionsAcceptance(User user, TermsConditionsInfo termsConditionsInfo, bool isAccepted, CancellationToken cancellationToken);
    Task<TermsConditionsAcceptance?> GetTermsConditionsAcceptedAsync(User user,
        TermsConditionsInfo termsConditionsInfo);
}
