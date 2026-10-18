using ECommerce.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Persistence.Configurations;

public sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(brand => brand.Id);
        builder.Property(brand => brand.Name).HasMaxLength(150).IsRequired();
        builder.Property(brand => brand.Slug).HasMaxLength(180).IsRequired();
        builder.Property(brand => brand.LogoUrl).HasMaxLength(2048);
        builder.HasIndex(brand => brand.Name).IsUnique();
        builder.HasIndex(brand => brand.Slug).IsUnique();
        builder.HasMany(brand => brand.Products).WithOne(product => product.Brand)
            .HasForeignKey(product => product.BrandId).OnDelete(DeleteBehavior.Restrict);
    }
}
