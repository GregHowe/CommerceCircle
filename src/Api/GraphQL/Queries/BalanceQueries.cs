using HotChocolate.Authorization;
using N1coLoyalty.Application.Balance.Queries;
using N1coLoyalty.Application.Common.Models;

namespace N1coLoyalty.Api.GraphQL.Queries;

[ExtendObjectType("Query")]
public class BalanceQueries
{
    [Authorize]
    [Serial]
    public async Task<BalanceDto> GetBalance([Service] IMediator mediator)
        => await mediator.Send(new GetBalanceQuery());
}
