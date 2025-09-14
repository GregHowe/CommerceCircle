namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyNotificationDto
{
    public required string Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required string FormattedMessage { get; set; }
}
