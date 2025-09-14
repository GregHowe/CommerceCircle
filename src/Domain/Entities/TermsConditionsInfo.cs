namespace N1coLoyalty.Domain.Entities;

public sealed class TermsConditionsInfo: BaseAuditableEntity
{
    public new Guid Id { get; set; }
    public required string Version { get; set; }
    public required string Url { get; set; }
    public bool IsCurrent { get; set; }
    public ICollection<TermsConditionsAcceptance> TermsConditionsAcceptances { get; set; } = new List<TermsConditionsAcceptance>();
}
