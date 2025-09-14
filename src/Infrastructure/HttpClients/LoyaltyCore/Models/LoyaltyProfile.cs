namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyProfile: LoyaltyProfileBase
{
    public WalletBalanceResponse? Balance { get; set; }
    public List<LoyaltyProgram> LoyaltyPrograms { get; set; } = [];
    public LoyaltyReferral? Referral { get; set; }
}
