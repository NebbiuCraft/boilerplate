using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderConfiguration : BaseEntityConfiguration<Order>
{
  protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
  {
    // Table name
    builder.ToTable("Orders");

    // Order-specific properties
    builder.Property(o => o.CustomerEmail)
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(o => o.IsPaid)
        .IsRequired()
        .HasDefaultValue(false);

    builder.Property(o => o.TransactionId)
        .HasMaxLength(100);

    builder.Property(o => o.PaymentDate)
        .IsRequired(false);

    builder.Property(o => o.TotalAmount)
        .HasColumnType("decimal(18,2)")
        .HasDefaultValue(0);

    // Configure the Items collection to use the backing field
    builder.Navigation(e => e.Items)
        .UsePropertyAccessMode(PropertyAccessMode.Field)
        .EnableLazyLoading(false);

    // Order-specific indexes
    builder.HasIndex(o => o.CustomerEmail)
        .HasDatabaseName("IX_Orders_CustomerEmail");

    builder.HasIndex(o => o.TransactionId)
        .HasDatabaseName("IX_Orders_TransactionId")
        .IsUnique()
        .HasFilter("[TransactionId] IS NOT NULL AND [TransactionId] != ''");

    builder.HasIndex(o => o.IsPaid)
        .HasDatabaseName("IX_Orders_IsPaid");
  }
}