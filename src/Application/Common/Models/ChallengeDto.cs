using N1coLoyalty.Application.Events.Commands;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Models;

public class ChallengeDto
{
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ChallengeTypeValue? Type { get; set; }
    public ChallengeStatusValue? Status { get; set; }
    public EffectDto? Effect { get; set; }
    public int? Target { get; set; }
    public int TargetProgress { get; set; }
    public List<StoreDto> Stores { get; set; } = [];
}
