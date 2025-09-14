namespace N1coLoyalty.Application.Common.Models;

public class LoyaltyProgramDto
{
    public required string IntegrationId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public virtual ICollection<TierDto> Tiers { get; set; } = new List<TierDto>();
}
