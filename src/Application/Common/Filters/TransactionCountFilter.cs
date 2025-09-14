using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Filters;

public class TransactionCountFilter
{
    public EffectTypeValue? TransactionType { get; set; }
    public TransactionOriginValue? TransactionOrigin { get; set; } 
}