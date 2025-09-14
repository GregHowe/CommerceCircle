namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class WalletBalanceResponse
{
    public decimal? Credit { get; set; }
    public decimal? Debit { get; set; }
    public string? TransactionId { get; set; }
    public decimal? HistoricalCredit { get; set; }
}
