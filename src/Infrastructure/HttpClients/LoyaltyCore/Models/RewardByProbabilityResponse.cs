namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class RewardByProbabilityResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public LoyaltyEffect? Effect { get; set; }
}
