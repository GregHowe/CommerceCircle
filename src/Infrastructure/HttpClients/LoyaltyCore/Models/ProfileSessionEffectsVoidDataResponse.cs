namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class ProfileSessionEffectsVoidDataResponse
{
    public required LoyaltyProfileSession OriginalProfileSession { get; set; }
    public required LoyaltyProfileSession VoidProfileSession { get; set; }
}
