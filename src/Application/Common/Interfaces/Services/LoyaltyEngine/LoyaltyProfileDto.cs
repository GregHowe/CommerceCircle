namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyProfileDto: LoyaltyProfileBaseDto
{
    public WalletBalanceResponseDto? Balance { get; set; }
    public List<LoyaltyProgramDto> LoyaltyPrograms { get; set; } = [];
    public LoyaltyReferralDto? Referral { get; set; }
}
