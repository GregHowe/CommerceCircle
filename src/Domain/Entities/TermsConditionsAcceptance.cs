namespace N1coLoyalty.Domain.Entities;

public sealed class TermsConditionsAcceptance: BaseAuditableEntity, ISoftDelete
{
    public new Guid Id { get; set; }
    public required Guid TermsConditionsId { get; set; }
    public required TermsConditionsInfo TermsConditionsInfo { get; set; }
    public required Guid UserId{ get; set; }
    public required User User { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsDeleted { get; set; }
}
