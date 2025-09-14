namespace N1coLoyalty.Application.Common.Models;
public class CommonServiceResponse<T>
{
    public string Message { get; set; } = "Operation done successfully";
    public bool Success { get; set; } = true;
    public string Code { get; set; } = "OK";
    public T? Data { get; set; }
}
