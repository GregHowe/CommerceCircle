using HotChocolate.Authorization;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Users.Queries;

namespace N1coLoyalty.Api.GraphQL.Queries;

[ExtendObjectType("Query")]
public class UserQueries
{
    [Authorize]
    public async Task<UserOnboardingCompletedDto?> UserOnboardingCompleted([Service] IMediator mediator)
        => await mediator.Send(new UserOnboardingCompletedQuery());
}