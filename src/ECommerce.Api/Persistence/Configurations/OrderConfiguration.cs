using ECommerce.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.OrderNumber).HasMaxLength(40).IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(order => order.PaymentProvider).HasMaxLength(24).IsRequired();
        builder.Property(order => order.PaymentReference).HasMaxLength(64).IsRequired();
        builder.Property(order => order.Currency).HasMaxLength(3).IsRequired();
        builder.Property(order => order.TotalAmount).HasPrecision(18, 2);
        builder.HasIndex(order => new { order.UserId, order.CreatedAt });
        builder.HasOne(order => order.User).WithMany().HasForeignKey(order => order.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(order => order.Items).WithOne(item => item.Order).HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "\"UnitPrice\" >= 0");
        });
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Sku).HasMaxLength(64).IsRequired();
        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.LineTotal).HasPrecision(18, 2);
        builder.HasIndex(item => new { item.OrderId, item.ProductId }).IsUnique();
    }
}
