namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyProfileSessionDto
{
    public required Guid Id { get; set; }
    public required string Status { get; set; } = string.Empty;
    public required LoyaltyProfileBaseDto Profile { get; set; }
    public required LoyaltyEventDto Event { get; set; }
    public required List<LoyaltyEffectDto> Effects { get; set; } = [];
}
