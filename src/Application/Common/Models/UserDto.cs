namespace N1coLoyalty.Application.Common.Models;

public class UserDto
{
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public string? ExternalUserId { get; set; }
    public string? Phone { get; set; }
}
