namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyEventDto
{
    public string? EventType { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
}
