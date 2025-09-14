using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Infrastructure.Data.Configurations;

public class TermsConditionsInfoConfiguration: IEntityTypeConfiguration<TermsConditionsInfo>
{
    public void Configure(EntityTypeBuilder<TermsConditionsInfo> builder)
    {
        builder.ToTable(@"TermsConditionsInfo", @"dbo");
        builder.Property(x => x.Id).HasColumnType(@"uniqueidentifier").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.Version).HasMaxLength(300).ValueGeneratedNever().IsRequired();
        builder.Property(x => x.Url).HasMaxLength(500).IsRequired().ValueGeneratedNever();
        builder.Property(x => x.IsCurrent).IsRequired().HasDefaultValue(0);
        
        // PK
        builder.HasKey(x => x.Id);
        builder.HasIndex(b => b.IsCurrent).IsUnique().HasFilter("IsCurrent = 1");

    }
}
