using N1coLoyalty.Domain.Enums.LoyaltyEngine;

namespace N1coLoyalty.Domain.Entities;

public class LoyaltyEffectAction
{
    public string? Type { get; set; }
    public decimal Amount { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

}