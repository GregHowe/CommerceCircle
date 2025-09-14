using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Transaction> Transactions { get; }
    
    DbSet<UserWalletBalance> UserWalletBalances { get; }
    DbSet<TermsConditionsInfo> TermsConditionsInfo { get; }
    DbSet<TermsConditionsAcceptance> TermsConditionsAcceptances { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    void RollbackTransaction();
}
