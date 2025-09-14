namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class ProfileSessionEffectsVoidResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ProfileSessionEffectsVoidDataResponse? Data { get; set; } = null;
}
