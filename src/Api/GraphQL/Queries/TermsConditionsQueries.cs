using HotChocolate.Authorization;
using N1coLoyalty.Application.TermsConditions.Queries;
using N1coLoyalty.Application.Common.Models;

namespace N1coLoyalty.Api.GraphQL.Queries;

[ExtendObjectType("Query")]
public class TermsConditionsQueries
{
    [Authorize]
    public async Task<TermsConditionsInfoDto?> GetTermsConditionsInfo([Service] IMediator mediator)
        => await mediator.Send(new GetTermsConditionsInfoQuery());
}
