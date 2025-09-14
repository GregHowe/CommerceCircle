namespace N1coLoyalty.Application.Common.Interfaces.Services.CashBack;

public class ApplyCashBackDto
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
    public required string Code { get; set; }
    public CashBackTransactionDto? CashBackTransaction { get; set; }
}
