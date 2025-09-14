namespace N1coLoyalty.Application.Common.Models;

public class ProfileDto
{
    public string? IntegrationId { get; set; }
    public string? PhoneNumber { get; set; }
    public BalanceDto Balance { get; set; } = new();
    public LoyaltyProgramDto? LoyaltyProgram { get; set; }
    public ReferralDto? Referral { get; set; }
    
    public bool IsNew { get; set; }
}
