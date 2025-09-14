using HotChocolate.Authorization;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Game.Queries;

namespace N1coLoyalty.Api.GraphQL.Queries;

[ExtendObjectType("Query")]

public class GameQueries
{
    [Authorize]
    [Serial]
    public async Task<GameSettingsDto> GetGameSettings([Service] IMediator mediator)
        => await mediator.Send(new GetGameSettingsQuery());
}
