namespace N1coLoyalty.Infrastructure.HttpClients.CashBack.Models;

public class ApplyCashBackRequest
{
    public required string PhoneNumber { get; set; }
    public required decimal AmountTrx { get; set; }
    public required string Reason { get; set; }
    public required string Description { get; set; }
    public required string OriginType { get; set; }
}
