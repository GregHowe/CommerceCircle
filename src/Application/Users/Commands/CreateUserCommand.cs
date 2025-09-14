using Microsoft.Extensions.Configuration;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Application.Profile.Services;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Users.Commands;

public class CreateUserCommand : IRequest<CommonServiceResponse<BalanceDto>>
{
    public class CreateUserCommandHandler(
        UserWalletService userWalletService,
        IUser currentUser,
        IConfiguration configuration,
        IUserRepository userRepository,
        ITransactionRepository transactionRepository,
        IApplicationDbContext context,
        ProfileService profileService)
        : IRequestHandler<CreateUserCommand, CommonServiceResponse<BalanceDto>>
    {
        public async Task<CommonServiceResponse<BalanceDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var commonServiceResponse = new CommonServiceResponse<BalanceDto>()
            {
                Success = false ,
                Message = "General Error",
                Code = "ERROR",
                Data = new BalanceDto {
                    AvailableCoins = 0,
                    AccumulatedCoins = 0
                }
            };

            var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);
            
            var loyaltyProgramIntegrationId = configuration["LoyaltyCore:LoyaltyProgramIntegrationId"] ?? string.Empty;
            var profile = await profileService.GetOrCreateProfile(user, loyaltyProgramIntegrationId);
            if (profile is null)
            {
                commonServiceResponse.Message = "Error al crear el perfil";
                commonServiceResponse.Code = "PROFILE_CREATION_ERROR";
                return commonServiceResponse;
            }

            var walletCreated = await context.UserWalletBalances.AnyAsync(wb =>
                wb.UserId == user.Id &&
                wb.Action == WalletActionValue.Create, cancellationToken: cancellationToken);
            if (!walletCreated)
            {
                var createWalletResponse = await userWalletService.CreateWallet(user);
                if (createWalletResponse is null)
                {
                    commonServiceResponse.Message = "Error al crear la cuenta";
                    commonServiceResponse.Code = "WALLET_CREATION_ERROR";
                    return commonServiceResponse;
                }
            }

            var hasOnboardingReward = await context.Transactions.AnyAsync(t =>
                t.UserId == user.Id && t.TransactionOrigin == TransactionOriginValue.Onboarding, cancellationToken);
            if (!hasOnboardingReward)
            {
                var rewardAmount = Convert.ToDecimal(configuration["Onboarding:Reward"]);
                var transactionModel = GetTransactionModel(user,rewardAmount);
                await context.BeginTransactionAsync();
                var transaction = await transactionRepository.CreateTransaction(transaction:transactionModel,cancellationToken: cancellationToken);
                var creditWalletResponse = await userWalletService.Credit(transaction);
                if (creditWalletResponse is null)
                {
                    context.RollbackTransaction();
                    commonServiceResponse.Message = "Error al acreditar la cuenta";
                    commonServiceResponse.Code = "WALLET_CREDIT_ERROR";
                    return commonServiceResponse;
                }
                await context.CommitTransactionAsync();
                
                commonServiceResponse.Data = new BalanceDto
                {
                    AvailableCoins = creditWalletResponse.Balance,
                    AccumulatedCoins = creditWalletResponse.HistoricalCredit
                };
                commonServiceResponse.Success = true;
                commonServiceResponse.Message = "Cuenta creada con éxito";
                commonServiceResponse.Code = "OK";

                return commonServiceResponse;
            }
            
            var walletBalanceResponse = await userWalletService.GetBalance(user);
            commonServiceResponse.Data = new BalanceDto
            {
                AvailableCoins = walletBalanceResponse?.Balance ?? 0,
                AccumulatedCoins = walletBalanceResponse?.HistoricalCredit ?? 0
            };
            commonServiceResponse.Success = false;
            commonServiceResponse.Message = "Cuenta ya había sido creada con éxito";
            commonServiceResponse.Code = "USER_ALREADY_EXISTS";

            return commonServiceResponse;
        }

        private static Transaction GetTransactionModel( User user, decimal rewardAmount)
        {
            return new Transaction()
            {
                Amount = rewardAmount,
                Name = TransactionName.OnboardingReward,
                Description = TransactionDescription.Onboarding,
                TransactionStatus = TransactionStatusValue.Redeemed,
                TransactionType = EffectTypeValue.Reward,
                TransactionSubType = EffectSubTypeValue.Point,
                TransactionOrigin = TransactionOriginValue.Onboarding,
                UserId = user.Id,
                User = user
            };
        }
    }
}
