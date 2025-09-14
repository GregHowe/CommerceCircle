namespace N1coLoyalty.Application.Transactions.Queries.GetAdminTransactions;

public class TransactionsAllVm
{
    public IQueryable<AdminTransactionDto> Transactions { get; set; }
}