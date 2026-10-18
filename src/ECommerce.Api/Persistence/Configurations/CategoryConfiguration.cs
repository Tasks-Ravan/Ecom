using ECommerce.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name).HasMaxLength(150).IsRequired();
        builder.Property(category => category.Slug).HasMaxLength(180).IsRequired();
        builder.HasIndex(category => category.Slug).IsUnique();
        builder.HasOne(category => category.ParentCategory).WithMany(category => category.Children)
            .HasForeignKey(category => category.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(category => category.Products).WithOne(product => product.Category)
            .HasForeignKey(product => product.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
