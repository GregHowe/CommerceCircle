using Microsoft.EntityFrameworkCore;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Infrastructure.Data.Repositories;

public class TermsConditionsRepository(
    IApplicationDbContext context
) : ITermsConditionsRepository
{
    public Task<TermsConditionsInfo?> GetCurrentTermsConditionsAsync()
    {
        return context.TermsConditionsInfo
            .Where(x => x.IsCurrent)
            .OrderByDescending(x => x.Created)
            .FirstOrDefaultAsync();
    }
}
