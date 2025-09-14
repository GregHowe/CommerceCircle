namespace N1coLoyalty.Domain.Entities;

public class Event
{
    public string? EventType { get; set; }

    public Dictionary<string, string>? Attributes { get; set; }
}
