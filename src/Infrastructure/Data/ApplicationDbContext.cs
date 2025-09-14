using System.Data;
using System.Reflection;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using N1coLoyalty.Domain.Common;
using N1coLoyalty.Infrastructure.Data.Extensions;

namespace N1coLoyalty.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<UserWalletBalance> UserWalletBalances => Set<UserWalletBalance>();
    public DbSet<TermsConditionsInfo> TermsConditionsInfo => Set<TermsConditionsInfo>();
    public DbSet<TermsConditionsAcceptance> TermsConditionsAcceptances => Set<TermsConditionsAcceptance>();
    private IDbContextTransaction? _currentTransaction;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    entry.Entity.IsDeleted = true;
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await base.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted).ConfigureAwait(false);
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync().ConfigureAwait(false);

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
            ChangeTracker.Clear();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            t => t.Namespace?.Contains("Configurations.LoyaltyEngine") == false);

        modelBuilder.SetQueryFilterOnAllEntities<ISoftDelete>(e => !e.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}
