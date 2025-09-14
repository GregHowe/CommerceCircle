namespace N1coLoyalty.Application.Common.Interfaces;

public class WalletBalanceResponseDto
{
    public decimal? Credit { get; set; }
    public decimal? Debit { get; set; }
    public string? TransactionId { get; set; }
    public decimal? HistoricalCredit { get; set; }
}
