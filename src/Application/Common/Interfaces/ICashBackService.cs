using N1coLoyalty.Application.Common.Interfaces.Services.CashBack;

namespace N1coLoyalty.Application.Common.Interfaces;

public interface ICashBackService
{
    Task<ApplyCashBackDto> ApplyCashBack(string phoneNumber, decimal amount, string reason, string description);
}
