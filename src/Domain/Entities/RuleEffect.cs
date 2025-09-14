namespace N1coLoyalty.Domain.Entities;

public class RuleEffect
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Status { get; set; }
    public LoyaltyEffectAction? Action { get; set; }
    public string? CampaignId { get; set; }
    public Reward? Reward { get; set; }
    public Challenge? Challenge { get; set; }
}
