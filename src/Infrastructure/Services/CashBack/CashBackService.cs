using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.CashBack;
using N1coLoyalty.Infrastructure.HttpClients.CashBack;

namespace N1coLoyalty.Infrastructure.Services.CashBack;

public class CashBackService(CashBackHttpClient cashBackHttpClient): ICashBackService
{
    public async Task<ApplyCashBackDto> ApplyCashBack(string phoneNumber, decimal amount, string reason, string description)
    {
        var applyCashBackResponse = await cashBackHttpClient.ApplyCashBackAsync(phoneNumber, amount, reason, description);
        return new ApplyCashBackDto()
        {
            Success = applyCashBackResponse.Success,
            Message = applyCashBackResponse.Message,
            Code = applyCashBackResponse.Code,
            CashBackTransaction = applyCashBackResponse.Data is not null ? new CashBackTransactionDto()
            {
                Id = applyCashBackResponse.Data.Id,
                Amount = applyCashBackResponse.Data.Amount,
                OriginTransactionId = applyCashBackResponse.Data.OriginTransactionId
            } : null
        };
    }
}
