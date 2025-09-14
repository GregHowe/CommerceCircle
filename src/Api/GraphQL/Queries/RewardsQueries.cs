using HotChocolate.Authorization;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Rewards.Queries;

namespace N1coLoyalty.Api.GraphQL.Queries;

[ExtendObjectType("Query")]

public class RewardsQueries
{
    [Authorize]
    [Serial]
    public async Task<List<RewardDto>> GetAvailableRewards([Service] IMediator mediator)
        => await mediator.Send(new GetAvailableRewardsQuery());
}
