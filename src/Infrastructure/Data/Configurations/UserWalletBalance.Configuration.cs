using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Infrastructure.Data.Configurations;

public class UserWalletBalanceConfiguration : IEntityTypeConfiguration<UserWalletBalance>
{
    public void Configure(EntityTypeBuilder<UserWalletBalance> builder)
    {
        builder.ToTable("UserWalletBalance", "dbo");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.Reason)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(e => e.Reference)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasPrecision(18, 5).IsRequired()
            .IsRequired();

        builder.Property(e => e.Action)
            .HasConversion(new EnumToStringConverter<WalletActionValue>())
            .HasMaxLength(20)
            .IsRequired();
        
        builder.HasOne(e => e.User)
            .WithMany(e => e.WalletBalances)
            .HasForeignKey(e => e.UserId)
            .IsRequired();
        
        builder.HasOne(x => x.Transaction)
            .WithOne(op => op.UserWalletBalance)
            .HasForeignKey<UserWalletBalance>(x => x.TransactionId)
            .IsRequired(false);
    }
}
