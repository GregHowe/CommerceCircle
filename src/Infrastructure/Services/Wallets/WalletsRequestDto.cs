using System.Text.Json.Serialization;

namespace N1coLoyalty.Infrastructure.Services.Wallets;

public class WalletsRequestDto
{
    [JsonPropertyName("profileIntegrationId")]
    public required string ProfileIntegrationId { get; set; }
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}
