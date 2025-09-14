namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class ProcessEventInputDto
{
    public required string LoyaltyProgramIntegrationId { get; set; }
    public required string EventType { get; set; }
    public required LoyaltyProfileDto LoyaltyProfile { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
}
