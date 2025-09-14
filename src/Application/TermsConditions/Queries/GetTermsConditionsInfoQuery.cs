using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Common.Interfaces;

namespace N1coLoyalty.Application.TermsConditions.Queries
{
    public class GetTermsConditionsInfoQuery : IRequest<TermsConditionsInfoDto>
    {
        public class GetTermsConditionsInfoQueriesHandles(
            ITermsConditionsRepository termsConditionsRepository,
            ITermsConditionsAcceptanceRepository termsConditionsAcceptanceRepository,
            IUser currentUser,
            IUserRepository userRepository
            ) : IRequestHandler<GetTermsConditionsInfoQuery, TermsConditionsInfoDto?>
        {
            public async Task<TermsConditionsInfoDto?> Handle(GetTermsConditionsInfoQuery request,
                CancellationToken cancellationToken)
            {
                var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);
                
                var termsConditions = await termsConditionsRepository.GetCurrentTermsConditionsAsync();
                
                if (termsConditions is null) return null;
                
                var acceptance = await termsConditionsAcceptanceRepository.GetTermsConditionsAcceptedAsync(user, termsConditions);

                return new TermsConditionsInfoDto()
                {
                    Id = termsConditions.Id,
                    Version = termsConditions.Version,
                    Url = termsConditions.Url,
                    IsAccepted = acceptance?.IsAccepted ?? false
                };
            }
        }
    }
}
