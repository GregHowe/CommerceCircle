namespace N1coLoyalty.Domain.Entities;

public class User : BaseAuditableEntity, ISoftDelete
{
    public new virtual Guid Id { get; set; }
    public virtual string? Name { get; set; }
    public virtual int FailedLoginAttempts { get; set; }
    public virtual bool IsBlocked { get; set; }
    public virtual string? AppRegistrationToken { get; set; }
    public virtual required string ExternalUserId { get; set; }
    public DateTime? LastSessionDate { get; set; }
    public string? Phone { get; set; }
    public bool OnboardingCompleted { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<UserWalletBalance> WalletBalances { get; set; } = new List<UserWalletBalance>();
    public virtual ICollection<TermsConditionsAcceptance> TermsConditionsAcceptances { get; set; } =
        new List<TermsConditionsAcceptance>();
    public bool IsDeleted { get; set; }
}
