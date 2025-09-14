using HotChocolate.Authorization;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Transactions.Queries.GetTransactions;

namespace N1coLoyalty.Api.GraphQL.Queries;

[ExtendObjectType("Query")]
public class TransactionQueries
{
    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Serial]
    public async Task<IQueryable<TransactionDto>> GetTransactions([Service] IMediator mediator)
    {
        var result = await mediator.Send(new GetTransactionsQuery());
        return result.Transactions;
    }
    
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Serial]
    public async Task<IQueryable<TransactionDto>> GetAllTransactions([Service] IMediator mediator)
    {
        var result = await mediator.Send(new GetTransactionsQuery());
        return result.Transactions;
    }
}
