using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : BaseEntityConfiguration<OrderItem>
{
  protected override void ConfigureEntity(EntityTypeBuilder<OrderItem> builder)
  {
    // Table name
    builder.ToTable("OrderItems");

    // OrderItem-specific properties
    builder.Property(oi => oi.ProductName)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(oi => oi.Quantity)
        .IsRequired()
        .HasDefaultValue(1);

    // Shadow property for foreign key to Order
    builder.Property<int>("OrderId")
        .IsRequired();

    // Configure the relationship from OrderItem side
    builder.HasOne<Order>()
        .WithMany(o => o.Items)
        .HasForeignKey("OrderId")
        .OnDelete(DeleteBehavior.Cascade);

    // OrderItem-specific indexes
    builder.HasIndex(oi => oi.ProductName)
        .HasDatabaseName("IX_OrderItems_ProductName");

    builder.HasIndex("OrderId")
        .HasDatabaseName("IX_OrderItems_OrderId");
  }
}