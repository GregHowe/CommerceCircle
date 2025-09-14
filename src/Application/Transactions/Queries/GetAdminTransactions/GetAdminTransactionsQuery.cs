using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Security;

namespace N1coLoyalty.Application.Transactions.Queries.GetAdminTransactions;

[Authorize(Permission.ReadTransactions)]
public class GetAdminTransactionsQuery : IRequest<TransactionsAllVm>
{
    public class GetAdminTransactionsQueryHandler(
        ITransactionRepository transactionRepository
        ) : IRequestHandler<GetAdminTransactionsQuery, TransactionsAllVm>
    {
        public async Task<TransactionsAllVm> Handle(GetAdminTransactionsQuery request, CancellationToken cancellationToken)
        {
            var queryableTransactions = transactionRepository.GetQueryableTransactions();

            var queryable = queryableTransactions.Select(t => new AdminTransactionDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Created = t.Created,
                Amount = t.Amount,
                Type = t.TransactionType,
                SubType = t.TransactionSubType,
                Status = t.TransactionStatus,
                Origin = t.TransactionOrigin,
                UserId = t.UserId,
                Metadata = t.Metadata,
                RuleEffect = t.RuleEffect
            });

            return await Task.FromResult(new TransactionsAllVm
            {
                Transactions = queryable
            });
        }
    }
}