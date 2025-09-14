using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Services.Void;

public class VoidService(
    ITransactionRepository transactionRepository,
    UserWalletService userWalletService,
    ILoyaltyEngine loyaltyEngine,
    IApplicationDbContext dbContext,
    ILoyaltyEventService loyaltyEventService)
{
    public async Task<ProcessVoidResponseDto> ProcessVoid(
        Guid transactionId,
        string reason,
        TransactionOriginValue origin,
        bool runAsync,
        CancellationToken cancellationToken)
    {
        var transactionOriginal = await transactionRepository.GetTransactionById(transactionId);

        var response = new ProcessVoidResponseDto();

        if (transactionOriginal.ProfileSessionId != null)
        {
            await VoidSession(transactionOriginal.User, transactionOriginal.ProfileSessionId, runAsync, response);
            return response;
        }

        var voidTransactionResponse = await VoidTransaction(reason, transactionOriginal, origin, cancellationToken);
        if (voidTransactionResponse is null) return response;
        response.Success = true;
        response.Message = "Transacción anulada correctamente";
        response.Code = "OK";
        
        return response;
    }

    private async Task VoidSession(User user, string profileSessionId, bool runAsync,
        ProcessVoidResponseDto response)
    {
        var voidSessionInput = new VoidSessionInputDto { ProfileSessionId = profileSessionId };

        if (runAsync)
        {
            var voidSessionAsyncResponse = await loyaltyEngine.VoidSessionAsync(voidSessionInput);
            response.Code = voidSessionAsyncResponse.Code;
            response.Message = voidSessionAsyncResponse.Message;
            response.Success = voidSessionAsyncResponse.Success;
            return;
        }

        var voidSessionResponse = await loyaltyEngine.VoidSession(voidSessionInput);
        response.Message = voidSessionResponse.Message;
        response.Success = voidSessionResponse.Success;
        response.Code = voidSessionResponse.Code;

        if (voidSessionResponse.Data is not null)
        {
            var loyaltyProfileSessionDto = voidSessionResponse.Data.VoidProfileSession;
            var voidProfileSessionId = loyaltyProfileSessionDto.Id.ToString();
            
            var originalTransactions = await dbContext.Transactions
                .Where(t => t.ProfileSessionId == profileSessionId)
                .ToListAsync();
            foreach (var originalTransaction in originalTransactions)
            {
                originalTransaction.TransactionStatus = TransactionStatusValue.Voided;
                originalTransaction.Metadata.Add(TransactionMetadata.VoidProfileSessionId, voidProfileSessionId);
            }
            
            var voidTransactions = await loyaltyEventService.ProcessEffects(loyaltyProfileSessionDto.Effects, user,
                new LoyaltyEventDto
                {
                    EventType = loyaltyProfileSessionDto.Event.EventType,
                    Attributes = loyaltyProfileSessionDto.Event.Attributes
                }, voidProfileSessionId);

            foreach (var voidTransaction in voidTransactions) 
                voidTransaction.Metadata.Add(TransactionMetadata.VoidedProfileSessionId, profileSessionId);
            
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task<UserWalletResponse?> VoidTransaction(string reason, Transaction transactionOriginal, TransactionOriginValue origin, CancellationToken cancellationToken)
    {
        var idUser = transactionOriginal.UserId;
        var user = await dbContext.Users.FirstAsync(x => x.Id == idUser, cancellationToken);

        var transactionVoid = GetVoidTransactionModel(user, transactionOriginal, reason, origin);
        await transactionRepository.CreateTransaction(transactionVoid, cancellationToken);
        return await userWalletService.Void(transactionOriginal, transactionVoid);
    }
    
    private static Transaction GetVoidTransactionModel(User user, Transaction originalTransaction, string reason, TransactionOriginValue origin)
    {
        return new Transaction()
        {
            Amount = originalTransaction.Amount,
            Name = originalTransaction.TransactionType == EffectTypeValue.Debit ? TransactionName.Refund : TransactionName.Revert,
            Description = reason,
            TransactionStatus = TransactionStatusValue.Redeemed,
            TransactionType = originalTransaction.TransactionType == EffectTypeValue.Debit ? EffectTypeValue.Refund : EffectTypeValue.Revert,
            TransactionSubType = EffectSubTypeValue.Point,
            TransactionOrigin = origin,
            UserId = user.Id,
            User = user,
            Metadata = new Dictionary<string, string>()
            {
                {TransactionMetadata.VoidedTransactionId, originalTransaction.Id.ToString()}
            }
        };
    }
}