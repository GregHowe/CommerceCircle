namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyCampaignDto
{
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
    public decimal ConsumedBudget { get; set; }
    public decimal WalletConversionRate { get; set; }
    public string? UserEventFrequency { get; set; }
    public int? UserEventFrequencyLimit { get; set; }
    public decimal EventCost { get; set; }
    public decimal ExtraAttemptCost { get; set; }
    public List<LoyaltyRewardDto> Rewards { get; set; } = [];
    
}
