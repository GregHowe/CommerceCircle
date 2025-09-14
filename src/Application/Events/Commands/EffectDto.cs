using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.Enums.LoyaltyEngine;

namespace N1coLoyalty.Application.Events.Commands;

public class EffectDto
{
    public EffectTypeValue? Type { get; set; }
    public EffectSubTypeValue? SubType { get; set; }
    public decimal Amount { get; set; }
}
