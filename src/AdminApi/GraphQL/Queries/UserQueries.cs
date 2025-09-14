using HotChocolate.Authorization;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Users.Queries;

namespace N1coLoyalty.AdminApi.GraphQL.Queries;

[ExtendObjectType("Query")]
public class UserQueries
{
    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<UserDto>> GetUsers([Service] IMediator mediator)
    => (await mediator.Send(new GetUsersQuery())).Users;
    
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<UserDto>> GetAllUsers([Service] IMediator mediator)
        => (await mediator.Send(new GetUsersQuery())).Users;
}

