using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Infrastructure.Data.Configurations;

public class TermsConditionsAcceptanceConfiguration: IEntityTypeConfiguration<TermsConditionsAcceptance>
{
    public void Configure(EntityTypeBuilder<TermsConditionsAcceptance> builder)
    {
        builder.ToTable(@"TermsConditionsAcceptance", @"dbo");
        builder.Property(x => x.Id).HasColumnType(@"uniqueidentifier").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnType(@"uniqueidentifier").ValueGeneratedNever();
        builder.Property(x => x.TermsConditionsId).HasColumnType(@"uniqueidentifier").ValueGeneratedNever();
        builder.Property(x => x.IsAccepted).IsRequired().HasDefaultValue(0);
        // PK
        builder.HasKey(x => x.Id);
        
        // FK
        builder.HasOne(x => x.User).WithMany(op => op.TermsConditionsAcceptances).IsRequired().HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.TermsConditionsInfo).WithMany(op => op.TermsConditionsAcceptances).IsRequired().HasForeignKey(x => x.TermsConditionsId);
    }
}
