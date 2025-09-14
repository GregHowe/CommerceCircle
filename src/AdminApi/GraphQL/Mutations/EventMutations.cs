using BusinessEvents.Contracts.Issuing;
using BusinessEvents.Contracts.Issuing.Models;
using HotChocolate.Authorization;
using N1coLoyalty.AdminApi.Common;
using N1coLoyalty.Application.IssuingEvents;

namespace N1coLoyalty.AdminApi.GraphQL.Mutations;

[ExtendObjectType("Mutations")]
public class EventMutations : MutationBase
{
    /// <summary>
    /// Send Transaction Status Changed Event
    /// </summary>
    /// <returns>PayloadResult</returns>
    [Authorize]
    public async Task<PayloadResult<TransactionStatusChangedPayload>> SendTransactionStatusChangeEvent(
        TransactionStatusChangedPayload input, [Service] IIssuingEventsBus issuingEventsBus)
    {
        try
        {
            if (IsProduction())
                return new PayloadResult<TransactionStatusChangedPayload>
                {
                    Success = false,
                    Message = "This operation is not allowed for production environment",
                    Code = "ERROR",
                    Data = input,
                };
            
            await issuingEventsBus.PublishAsync(new TransactionStatusChange
            {
                Id = input.Id,
                Amount = input.Amount,
                TransactionStatus = input.TransactionStatus,
                ApprovalStatus = input.ApprovalStatus,
                TransactionType = input.TransactionType,
                OperationType = input.OperationType,
                User = new User
                {
                    Name = input.User.Name,
                    PhoneNumber = input.User.PhoneNumber,
                    ExternalUserId = input.User.ExternalUserId
                },
                PosMetadata = input.PosMetadata != null ? new PosMetadata
                {
                    MerchantId = input.PosMetadata.MerchantId,
                    TerminalId = input.PosMetadata.TerminalId,
                    Mcc = input.PosMetadata.Mcc,
                } : null
            });
            return new PayloadResult<TransactionStatusChangedPayload>
            {
                Success = true, Message = "Event sent", Code = "OK", Data = input,
            };
        }
        catch (Exception e)
        {
            return new PayloadResult<TransactionStatusChangedPayload>
            {
                Success = false, Message = e.Message, Code = "ERROR", Data = input,
            };
        }
    }

    private static bool IsProduction()
    {
        const string envKeyName = "DOTNET_ENVIRONMENT";
        var environment = Environment.GetEnvironmentVariable(envKeyName)?.ToLower();
        if (string.IsNullOrWhiteSpace(environment))
            throw new InvalidOperationException($"Environment variable {envKeyName} is not set.");
        
        return string.Equals(environment, Environments.Production, StringComparison.OrdinalIgnoreCase);
    }
}
