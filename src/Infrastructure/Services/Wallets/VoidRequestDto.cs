using System.Text.Json.Serialization;

namespace N1coLoyalty.Infrastructure.Services.Wallets;

public class VoidRequestDto
{
    [JsonPropertyName("profileIntegrationId")]
    public required string ProfileIntegrationId { get; set; }
    [JsonPropertyName("reference")]
    public required string Reference { get; set; }
}
