namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyEffect
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public required string Type { get; set; }
    public required string Status { get; set; } = string.Empty;
    public required LoyaltyAction Action { get; set; }
    public required string CampaignId { get; set; }
    public LoyaltyNotification? Notification { get; set; }
    public LoyaltyReward? Reward { get; set; }
    public LoyaltyChallenge? Challenge { get; set; }
}
