using HotChocolate.Authorization;
using N1coLoyalty.AdminApi.Common;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.UserWalletBalances.Commands.UpdateUserWalletBalance;

namespace N1coLoyalty.AdminApi.GraphQL.Mutations;

[ExtendObjectType("Mutations")]
public class UserWalletBalanceMutations : MutationBase
{
    /// <summary>
    /// Update User Wallet Balance
    /// </summary>
    /// <returns>PayloadResult</returns>
    [Authorize]
    public async Task<PayloadResult<UpdateUserWalletResponseDto>> UpdateUserWalletBalance(UpdateUserWalletBalanceCommand input, [Service] IMediator mediator)
        => await ResolveAsResponse(mediator, input);
}