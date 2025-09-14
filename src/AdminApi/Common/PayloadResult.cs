namespace N1coLoyalty.AdminApi.Common;

public class PayloadResult
{
    public bool Success { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public IList<UserError>? UserErrors { get; set; }
}

public class PayloadResult<T> : PayloadResult
{
    public T? Data { get; set; }
}
