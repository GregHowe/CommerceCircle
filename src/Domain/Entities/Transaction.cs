namespace N1coLoyalty.Domain.Entities;

public sealed class Transaction: BaseAuditableEntity, ISoftDelete
{
    public new Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public TransactionStatusValue TransactionStatus { get; set; }
    public EffectTypeValue TransactionType { get; set; }
    public EffectSubTypeValue TransactionSubType { get; set; }
    public decimal Amount { get; set; }
    public Guid UserId { get; set; }
    public required User User { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool IsDeleted { get; set; }
    public RuleEffect? RuleEffect { get; set; }
    public Event? Event { get; set; }
    public UserWalletBalance? UserWalletBalance { get; set; }
    public TransactionOriginValue TransactionOrigin { get; set; }
    public string? IntegrationId { get; set; }
    public string? ProfileSessionId { get; set; }
}
