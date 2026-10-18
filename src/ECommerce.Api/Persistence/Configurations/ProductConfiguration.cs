using System.Text.Json;
using ECommerce.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ECommerce.Api.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Sku).HasMaxLength(64).IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique();
        builder.Property(product => product.Price).HasPrecision(18, 2);
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        builder.Property(product => product.Translations)
            .HasColumnName("translations")
            .HasColumnType("jsonb")
            .HasConversion(new ValueConverter<Dictionary<string, ProductTranslation>, string>(
                value => JsonSerializer.Serialize(value, jsonOptions),
                value => JsonSerializer.Deserialize<Dictionary<string, ProductTranslation>>(value, jsonOptions) ?? new()));
        builder.Property(product => product.Translations).Metadata.SetValueComparer(new ValueComparer<Dictionary<string, ProductTranslation>>(
            (left, right) => Serialize(left) == Serialize(right),
            value => Serialize(value).GetHashCode(),
            value => Deserialize(Serialize(value))));
        builder.HasIndex(product => product.Translations).HasMethod("gin");
        builder.HasIndex(product => new { product.CategoryId, product.BrandId, product.Price, product.IsActive });
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static string Serialize(Dictionary<string, ProductTranslation>? value) =>
        JsonSerializer.Serialize(value ?? new Dictionary<string, ProductTranslation>(), JsonOptions);

    private static Dictionary<string, ProductTranslation> Deserialize(string value) =>
        JsonSerializer.Deserialize<Dictionary<string, ProductTranslation>>(value, JsonOptions) ?? new();
}
