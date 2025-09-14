using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Common.Models;

public class BenefitDto
{
    public required Guid Id { get; set; }
    public string? Description { get; set; }
    public BenefitTypeValue? Type { get; set; }
}
