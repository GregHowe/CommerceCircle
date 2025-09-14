using HotChocolate.Authorization;
using N1coLoyalty.Api.Common;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Users.Commands;

namespace N1coLoyalty.Api.GraphQL.Mutations;

[ExtendObjectType("Mutations")]
public class UserMutations : MutationBase
{
    /// <summary>
    /// Create User with wallet
    /// </summary>
    /// <returns>PayloadResult</returns>
    [Authorize]
    public async Task<PayloadResult<BalanceDto>> CreateUser([Service] IMediator mediator)
        => await ResolveAsResponse(mediator, new CreateUserCommand());
}
