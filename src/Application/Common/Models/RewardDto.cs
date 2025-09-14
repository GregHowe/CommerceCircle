using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Models;

public class RewardDto
{
    public required Guid Id { get; set; }
    public string? Description { get; set; }
    
    public EffectSubTypeValue SubType { get; set; }
}
