namespace N1coLoyalty.Infrastructure.HttpClients.CashBack.Models;

public class ApplyCashBackResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
    public required string Code { get; set; }
    public TransactionDto? Data { get; set; }
}
