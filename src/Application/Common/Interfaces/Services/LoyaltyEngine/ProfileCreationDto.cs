namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class ProfileCreationDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public LoyaltyProfileDto? Profile { get; set; }
}
