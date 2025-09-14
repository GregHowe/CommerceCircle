namespace N1coLoyalty.Application.Common.Services.Void;

public class ProcessVoidResponseDto
{
    public string Message { get; set; } = "El proceso de anulación ha fallado";
    public bool Success { get; set; }
    public string Code { get; set; } = "ERROR";
}
