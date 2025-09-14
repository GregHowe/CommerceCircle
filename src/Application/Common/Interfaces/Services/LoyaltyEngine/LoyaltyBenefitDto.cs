using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class LoyaltyBenefitDto
{
    public required string Id { get; set; }
    public required string Description { get; set; }
    public required BenefitTypeValue Type { get; set; }
}
