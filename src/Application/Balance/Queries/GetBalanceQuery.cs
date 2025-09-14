using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Common.Services;

namespace N1coLoyalty.Application.Balance.Queries;

public class GetBalanceQuery: IRequest<BalanceDto>
{
    public class GetBalanceQueryHandler(
        UserWalletService userWalletService,
        IUserRepository userRepository,
        IUser currentUser) : IRequestHandler<GetBalanceQuery, BalanceDto>
    {
        public async Task<BalanceDto> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);

            var balance = await userWalletService.GetBalance(user);

            return new BalanceDto
            {
                AvailableCoins = balance?.Balance ?? 0,
                AccumulatedCoins = balance?.HistoricalCredit ?? 0
            };
        }
    }
}


