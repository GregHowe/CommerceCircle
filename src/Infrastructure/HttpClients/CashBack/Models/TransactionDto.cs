namespace N1coLoyalty.Infrastructure.HttpClients.CashBack.Models;

public class TransactionDto
{
    public string? Id { get; set; }
    public string? OriginTransactionId { get; set; }
    public decimal? Amount { get; set; }
}
