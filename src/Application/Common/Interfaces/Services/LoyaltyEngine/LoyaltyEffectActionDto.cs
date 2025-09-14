namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyEffectActionDto
{
    public required string Type { get; set; }
    public decimal Amount { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
