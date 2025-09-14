using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Transactions.Queries.GetAdminTransactions;

public class AdminTransactionDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public EffectTypeValue Type { get; set; }
    public EffectSubTypeValue SubType { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatusValue Status { get; set; }
    public TransactionOriginValue Origin { get; set; }
    public Guid UserId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public RuleEffect? RuleEffect { get; set; }
}
