using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Infrastructure.Data.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(@"User", @"dbo");
        builder.Property(x => x.Id).HasColumnType(@"uniqueidentifier").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.FailedLoginAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsBlocked).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.AppRegistrationToken).ValueGeneratedNever().HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.ExternalUserId).HasMaxLength(300).IsRequired();
        builder.Property(x => x.LastSessionDate).ValueGeneratedNever();
        builder.Property(x => x.Phone).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.OnboardingCompleted).IsRequired().HasDefaultValue(0);
        
        // PK / FK
        builder.HasKey(@x => x.Id);
    }
}
