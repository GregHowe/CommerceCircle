namespace N1coLoyalty.Application.Common.Models;

public class UpdateUserWalletResponseDto
{
    public Guid? TransactionId { get; set; }
    public string? Reference { get; set; }
    public decimal? Balance { get; set; }
    public decimal? HistoricalCredit { get; set; }
}
