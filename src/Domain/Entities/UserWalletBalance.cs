namespace N1coLoyalty.Domain.Entities;

public sealed class UserWalletBalance : BaseAuditableEntity, ISoftDelete
{
    public new Guid Id { get; set; }
    public required string Reason { get; set; }
    public WalletActionValue Action { get; set; }
    public decimal Amount { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public required string Reference { get; set; }
    public Guid? TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
    public bool IsDeleted { get; set; }
}
