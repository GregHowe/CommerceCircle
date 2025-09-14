using HotChocolate.Authorization;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Profile.Queries;

namespace N1coLoyalty.Api.GraphQL.Queries;
[ExtendObjectType("Query")]
public class ProfileQueries
{
    [Authorize]
    [Serial]
    public async Task<ProfileDto?> GetProfile([Service] IMediator mediator)
        => await mediator.Send(new GetProfileQuery());
}
