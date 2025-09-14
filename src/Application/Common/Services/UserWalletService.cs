using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.Events;
namespace N1coLoyalty.Application.Common.Services;

public class UserWalletService(IWalletsService walletsService, ITermsConditionsRepository termsConditionsRepository, ITermsConditionsAcceptanceRepository termsConditionsAcceptanceRepository ,IApplicationDbContext context)
{
    public async Task<UserWalletResponse?> Credit(Transaction transaction)
    {
        var response = await walletsService.Credit(transaction.User.ExternalUserId, transaction.Amount);
        if (response is null) return null;

        transaction.User.AddDomainEvent(new UserWalletBalanceEvent
        {
            UserId = transaction.User.Id,
            Action = WalletActionValue.Credit,
            Amount = transaction.Amount,
            Reason = transaction.Name,
            Reference = response.TransactionId ?? string.Empty,
            TransactionId = transaction.Id,
        });
        await context.SaveChangesAsync(CancellationToken.None);

        return new UserWalletResponse()
        {
            Balance = response.Credit - response.Debit,
            TransactionId = response.TransactionId,
            HistoricalCredit = response.HistoricalCredit
        };
    }
    
    public async Task<UserWalletResponse?> Void(Transaction originalTransaction, Transaction voidTransaction)
    {
        var walletBalance = await context.UserWalletBalances.SingleOrDefaultAsync(x=>x.TransactionId==originalTransaction.Id);
        if (walletBalance is null) return null;

        var response = await walletsService.Void(originalTransaction.User.ExternalUserId,walletBalance.Reference);
        if (response is null) return null;

        var actionVoid = walletBalance.Action switch
        {
            WalletActionValue.Debit => WalletActionValue.DebitVoid,
            WalletActionValue.Credit => WalletActionValue.CreditVoid,
            _ => throw new ArgumentOutOfRangeException()
        };

        voidTransaction.User.AddDomainEvent(new UserWalletBalanceEvent
        {
            UserId = voidTransaction.User.Id,
            Action = actionVoid,
            Amount = voidTransaction.Amount,
            Reason = voidTransaction.Name,
            Reference = response.TransactionId ?? string.Empty,
            TransactionId = voidTransaction.Id,
        });

        originalTransaction.Metadata.TryAdd(TransactionMetadata.VoidTransactionId, voidTransaction.Id.ToString());
        originalTransaction.TransactionStatus = TransactionStatusValue.Voided;
        await context.SaveChangesAsync(CancellationToken.None);
        
        return new UserWalletResponse()
        {
            Balance = response.Credit - response.Debit,
            TransactionId = response.TransactionId,
            HistoricalCredit = response.HistoricalCredit
        };
    }

    public async Task<UserWalletResponse?> GetBalance(User user)
    {
        var response = await walletsService.GetBalance(user.ExternalUserId);
        if (response is null) return null;
        return new UserWalletResponse()
        {
            Balance = response.Credit - response.Debit,
            TransactionId = response.TransactionId,
            HistoricalCredit = response.HistoricalCredit
        };
    } 

    public async Task<UserWalletResponse?> CreateWallet(User user)
    {
        var response = await walletsService.CreateWallet(user.ExternalUserId);

        if (response is null) return null;

        user.AddDomainEvent(new UserWalletBalanceEvent
        {
            UserId = user.Id,
            Action = WalletActionValue.Create,
            Amount = 0m,
            Reason = "Creación de cuenta",
            Reference = response.TransactionId ?? string.Empty
        });
        await context.SaveChangesAsync(CancellationToken.None);

        return new UserWalletResponse()
        {
            Balance = response.Credit - response.Debit,
            TransactionId = response.TransactionId,
            HistoricalCredit = response.HistoricalCredit
        };
    }

    private async Task<CommonServiceResponse<UserWalletResponse?>?> TermsConditionsInfoValidations(User user)
    {
        var termsConditions = await termsConditionsRepository.GetCurrentTermsConditionsAsync();
                
        if (termsConditions is null)
            return new CommonServiceResponse<UserWalletResponse?>()
            {
                Success = false,
                Code = "TERMS_CONDITIONS_DOESNT_EXIST",
                Message = "Error general: Terminos y Condiciones no registradas",
                Data = null
            };

        var acceptance = await termsConditionsAcceptanceRepository.GetTermsConditionsAcceptedAsync(user, termsConditions);
        
        if (acceptance is null)
            return new CommonServiceResponse<UserWalletResponse?>()
            {
                Success = false,
                Code = "TERMS_CONDITIONS_NOT_ACCEPTED",
                Message = "Error general: Terminos y Condiciones no aceptadas",
                Data = null
            };
        return null;
    }

    public async Task<CommonServiceResponse<UserWalletResponse?>> Debit(Transaction transaction)
    {
        var termsConditionsInfoValidationsValue = await TermsConditionsInfoValidations(transaction.User);
        
        if (termsConditionsInfoValidationsValue is not null)
            return termsConditionsInfoValidationsValue;

        var response = await walletsService.Debit(transaction.User.ExternalUserId, transaction.Amount);
        if (response is null) return new CommonServiceResponse<UserWalletResponse?>()
        {
            Success = false,
            Code = "WALLET_SERVICE_DEBIT_ERROR",
            Message = "Error general: No se pudo debitar el costo de la ruleta",
            Data = null
        };

        transaction.User.AddDomainEvent(new UserWalletBalanceEvent
        {
            UserId = transaction.User.Id,
            Action = WalletActionValue.Debit,
            Amount = transaction.Amount,
            Reason = transaction.Name,
            Reference = response.TransactionId ?? string.Empty,
            TransactionId = transaction.Id,
        });
        await context.SaveChangesAsync(CancellationToken.None);

        return new CommonServiceResponse<UserWalletResponse?>()
        {
            Data = new UserWalletResponse()
            {
                Balance = response.Credit - response.Debit,
                TransactionId = response.TransactionId,
                HistoricalCredit = response.HistoricalCredit
            }
        };
    }
}
