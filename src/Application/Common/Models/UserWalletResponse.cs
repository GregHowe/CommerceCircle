namespace N1coLoyalty.Application.Common.Models;

public class UserWalletResponse
{
    public decimal? Balance { get; set; }
    public string? TransactionId { get; set; }
    public decimal? HistoricalCredit { get; set; }
}
