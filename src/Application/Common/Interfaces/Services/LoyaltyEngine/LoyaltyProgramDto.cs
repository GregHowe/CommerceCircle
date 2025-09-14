namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyProgramDto
{
    public required string Id { get; set; }
    public required string IntegrationId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<LoyaltyTierDto> Tiers { get; set; } = new();
}
