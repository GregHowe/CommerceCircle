using HotChocolate.Authorization;
using N1coLoyalty.Application.Transactions.Queries.GetAdminTransactions;

namespace N1coLoyalty.AdminApi.GraphQL.Queries;

[ExtendObjectType("Query")]

public class TransactionQueries
{
    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<AdminTransactionDto>> GetTransactions([Service] IMediator mediator)
    => (await mediator.Send(new GetAdminTransactionsQuery())).Transactions;

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<AdminTransactionDto>> GetAllTransactions([Service] IMediator mediator)
        => (await mediator.Send(new GetAdminTransactionsQuery())).Transactions;
}