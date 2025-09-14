using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;

namespace N1coLoyalty.Application.Transactions.Queries.GetTransactions;

public class GetTransactionsQuery: IRequest<GetTransactionsVm>
{
    public class GetTransactionsQueryHandler(
        ITransactionRepository transactionRepository,
        IUser currentUser,
        IUserRepository userRepository)
        : IRequestHandler<GetTransactionsQuery, GetTransactionsVm>
    {
        public async Task<GetTransactionsVm> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);
            var transactions = transactionRepository.GetTransactionsByUser(user.Id);
        
            return new GetTransactionsVm
            {
                Transactions = transactions
            };
        }
    }
}


