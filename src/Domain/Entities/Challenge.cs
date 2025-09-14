namespace N1coLoyalty.Domain.Entities;

public class Challenge
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Type { get; set; }
    public int? Target { get; set; }
    public decimal? EffectValue { get; set; }
    public required string EffectType { get; set; }
    public required string EffectSubType { get; set; }
}
