namespace N1coLoyalty.Domain.Entities;

public class Reward
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public required string IntegrationId { get; set; }
}
