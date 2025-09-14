namespace N1coLoyalty.Application.Events.Commands;

public record ProcessEffectActionResponse
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal? CurrentBalance { get; set; }
}
