using FluentValidation.Results;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Common.Security;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.Entities;
using ValidationException = N1coLoyalty.Application.Common.Exceptions.ValidationException;

namespace N1coLoyalty.Application.UserWalletBalances.Commands.UpdateUserWalletBalance;

[Authorize(Permission.WriteUserWalletBalance)]
public class UpdateUserWalletBalanceCommand : IRequest<CommonServiceResponse<UpdateUserWalletResponseDto>>
{
    public required string UserPhone { get; set; }
    public required WalletOperationValue Operation { get; set; }
    public required string Reason { get; set; }
    public required int Amount { get; set; }

    public class UpdateUserWalletBalanceCommandHandler(
        UserWalletService userWalletService,
        ITransactionRepository transactionRepository,
        IApplicationDbContext dbContext)
      : IRequestHandler<UpdateUserWalletBalanceCommand, CommonServiceResponse<UpdateUserWalletResponseDto>>
    {
        public async Task<CommonServiceResponse<UpdateUserWalletResponseDto>> Handle(UpdateUserWalletBalanceCommand request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Phone == request.UserPhone, cancellationToken);
            if (user is null)
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("UserPhone", "El usuario no existe")
                });

            await dbContext.BeginTransactionAsync();
            var transactionModel = GetTransactionModel(user, request);
            var transaction = await transactionRepository.CreateTransaction(transactionModel, cancellationToken);

            var walletResponse = request.Operation switch
            {
                WalletOperationValue.Credit => await userWalletService.Credit(transaction),
                WalletOperationValue.Debit => (await userWalletService.Debit(transaction)).Data,
                _ => throw new ValidationException(new List<ValidationFailure>
                {
                    new("Operation", "Operación no válida")
                })
            };

            if (walletResponse is null)
            {
                dbContext.RollbackTransaction();
                return new CommonServiceResponse<UpdateUserWalletResponseDto>
                {
                    Success = false,
                    Message = "No se pudo actualizar la wallet",
                    Code = "ERROR"
                };
            }
            
            await dbContext.CommitTransactionAsync();
            return new CommonServiceResponse<UpdateUserWalletResponseDto>
            {
                Success = true,
                Message = "Wallet actualizada exitosamente",
                Code = "OK",
                Data = new UpdateUserWalletResponseDto
                {
                    TransactionId = transaction.Id,
                    Reference = walletResponse.TransactionId,
                    Balance = walletResponse.Balance,
                    HistoricalCredit = walletResponse.HistoricalCredit,
                }
            };
        }

        private static Transaction GetTransactionModel(User user, UpdateUserWalletBalanceCommand request)
        {
            return new Transaction
            {
                TransactionOrigin = TransactionOriginValue.Admin,
                TransactionStatus = TransactionStatusValue.Redeemed,
                TransactionType = request.Operation == WalletOperationValue.Credit ? EffectTypeValue.Credit : EffectTypeValue.Debit,
                TransactionSubType = EffectSubTypeValue.Point,
                Amount = request.Amount,
                Name = TransactionName.UpdateUserWalletBalance,
                Description = request.Reason,
                UserId = user.Id,
                User = user
            };
        }
    }
}