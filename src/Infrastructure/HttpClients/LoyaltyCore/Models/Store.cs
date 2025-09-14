namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

public class Store
{
    /// <summary>
    /// The unique identifier of the store.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The name of the store.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the store.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The image url of the store.
    /// </summary>
    public string? ImageUrl { get; set; }
}