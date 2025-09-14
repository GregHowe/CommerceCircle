namespace N1coLoyalty.Domain.Events;

public class UserWalletBalanceEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public required string Reason { get; set; }
    public WalletActionValue Action { get; set; }
    public required string Reference { get; set; }
    public Guid? TransactionId { get; set; }
    public decimal Amount { get; set; }
}
