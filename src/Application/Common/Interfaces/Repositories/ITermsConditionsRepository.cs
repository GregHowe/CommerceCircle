using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.Common.Interfaces.Repositories;

public interface ITermsConditionsRepository
{
    Task<TermsConditionsInfo?> GetCurrentTermsConditionsAsync();
}
