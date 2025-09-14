namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class VoidSessionDataResponseDto
{
    public required LoyaltyProfileSessionDto OriginalProfileSession { get; set; }
    public required LoyaltyProfileSessionDto VoidProfileSession { get; set; }
}
