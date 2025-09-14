using N1coLoyalty.Application.Common.Models;

namespace N1coLoyalty.Application.Transactions.Queries.GetTransactions;

public class GetTransactionsVm
{
    public required IQueryable<TransactionDto> Transactions { get; set; }
}
