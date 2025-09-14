using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Infrastructure.Data.Extensions;

namespace N1coLoyalty.Infrastructure.Data.Configurations;

public class TransactionConfiguration: IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable(@"Transaction", @"dbo");
        builder.Property(x => x.Id).HasColumnType(@"uniqueidentifier").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasMaxLength(300).ValueGeneratedNever().IsRequired(false);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false).ValueGeneratedNever();
        builder.Property(x => x.Amount).HasPrecision(18, 5).IsRequired().ValueGeneratedNever().HasPrecision(18, 5);
        builder.Property(x => x.UserId).HasColumnType(@"uniqueidentifier").ValueGeneratedNever();
        builder.Property(x => x.Metadata).ValueGeneratedNever().HasJsonConversion();
        builder.Property(x => x.RuleEffect).ValueGeneratedNever().HasJsonConversion();
        builder.Property(x => x.Event).ValueGeneratedNever().HasJsonConversion();
        builder.Property(x => x.IntegrationId).ValueGeneratedNever().HasMaxLength(300).IsRequired(false);
        builder.Property(x => x.ProfileSessionId).ValueGeneratedNever().HasMaxLength(300).IsRequired(false);
        
        // Enums
        builder.Property(x => x.TransactionStatus)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<TransactionStatusValue>())
            .HasMaxLength(50);
        
        builder.Property(x => x.TransactionOrigin)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<TransactionOriginValue>())
            .HasMaxLength(50);
        
        builder.Property(e => e.TransactionType)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<EffectTypeValue>())
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(e => e.TransactionSubType)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<EffectSubTypeValue>())
            .HasMaxLength(50)
            .IsRequired();
        
        // PK
        builder.HasKey(x => x.Id);
        
        // FK
        builder.HasOne(x => x.User).WithMany(op => op.Transactions).IsRequired().HasForeignKey(x => x.UserId);
    }
}
