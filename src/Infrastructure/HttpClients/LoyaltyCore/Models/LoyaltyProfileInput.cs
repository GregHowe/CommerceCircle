namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class LoyaltyProfileInput
{
    public required string IntegrationId { get; set; }
    public string? PhoneNumber { get; set; }
    public required string LoyaltyProgramIntegrationId { get; set; }
}
