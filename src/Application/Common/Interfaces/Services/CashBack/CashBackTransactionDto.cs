namespace N1coLoyalty.Application.Common.Interfaces.Services.CashBack;

public class CashBackTransactionDto
{
    public string? Id { get; set; }
    public string? OriginTransactionId { get; set; }
    public decimal? Amount { get; set; }
}
