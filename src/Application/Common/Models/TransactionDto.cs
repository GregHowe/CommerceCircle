using N1coLoyalty.Application.Transactions.Queries.GetTransactions;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Models;

public class TransactionDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public EffectTypeValue Type { get; set; }
    public EffectSubTypeValue SubType { get; set; }
    public TransactionOperationTypeValue OperationType { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatusValue Status { get; set; }
    public TransactionTag Tag { get; set; }
    public TransactionOriginValue Origin { get; set; }
    public IconCategoryValue IconCategory { get; set; }
}
