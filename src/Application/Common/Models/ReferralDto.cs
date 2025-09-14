namespace N1coLoyalty.Application.Common.Models;

public class ReferralDto
{
    public required string Code { get; set; }
    public required bool IsActive { get; set; }
    public required int RewardAmount { get; set; }
}
