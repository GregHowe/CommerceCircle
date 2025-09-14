namespace N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;

public class VoidSessionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public VoidSessionDataResponseDto? Data { get; set; } = null;
}
