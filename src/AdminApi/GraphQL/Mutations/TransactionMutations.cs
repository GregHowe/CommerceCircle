using HotChocolate.Authorization;
using N1coLoyalty.AdminApi.Common;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Transactions.Commands.VoidTransaction;

namespace N1coLoyalty.AdminApi.GraphQL.Mutations;

[ExtendObjectType("Mutations")]
public class TransactionMutations : MutationBase
{
    /// <summary>
    /// Void Transaction
    /// </summary>
    /// <returns>PayloadResult</returns>
    [Authorize]
    public async Task<PayloadResult<object>> VoidTransaction(VoidTransactionCommand input, [Service] IMediator mediator)
        => await ResolveAsResponse(mediator, input);
}
