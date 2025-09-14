using Microsoft.EntityFrameworkCore;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Infrastructure.Data.Repositories;

public class TermsConditionsAcceptanceRepository(IApplicationDbContext context): ITermsConditionsAcceptanceRepository
{
    public async Task<TermsConditionsAcceptance> SaveTermsConditionsAcceptance(User user, TermsConditionsInfo termsConditionsInfo, bool isAccepted,
        CancellationToken cancellationToken)
    {
        var termsConditionsAcceptance = new TermsConditionsAcceptance
            {
                UserId = user.Id,
                User = user,
                TermsConditionsId = termsConditionsInfo.Id,
                TermsConditionsInfo = termsConditionsInfo,
                IsAccepted = isAccepted
            };

        await context.TermsConditionsAcceptances.AddAsync(termsConditionsAcceptance, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return termsConditionsAcceptance; 
    }

    public Task<TermsConditionsAcceptance?> GetTermsConditionsAcceptedAsync(User user,
        TermsConditionsInfo termsConditionsInfo)
    {
        return context.TermsConditionsAcceptances
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.TermsConditionsId == termsConditionsInfo.Id && x.IsAccepted);
    }
}
