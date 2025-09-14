namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class RewardByProbabilityInputDto
{
    public required string LoyaltyProgramIntegrationId { get; set; }
    public required string CampaignIntegrationId { get; set; }
    public required LoyaltyProfileDto LoyaltyProfile { get; set; }
}