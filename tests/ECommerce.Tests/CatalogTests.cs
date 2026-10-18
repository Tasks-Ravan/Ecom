using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Features.Categories;
using ECommerce.Api.Features.Products;
using ECommerce.Api.Persistence;
using Microsoft.AspNetCore.Http;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests;

public sealed class CatalogTests
{
    [Fact]
    public async Task CreateProductValidatorRejectsNegativePriceAndEmptyTranslations()
    {
        await using var db = CreateDbContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Phones", Slug = "phones" };
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Example", Slug = "example" };
        db.AddRange(category, brand);
        await db.SaveChangesAsync();
        var validator = new ProductValidator(db);
        var request = new ProductRequest("SKU-1", -0.01m, 0, category.Id, brand.Id, true,
            new Dictionary<string, ProductTranslation> { ["en"] = new("", "Description") });

        var result = await validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == "Price");
        Assert.Contains(result.Errors, error => error.PropertyName.Contains("Translations", StringComparison.Ordinal));
    }

    [Fact]
    public void TranslationResolverUsesRequestedThenEnglishThenFirstAvailable()
    {
        var translations = new Dictionary<string, ProductTranslation>
        {
            ["az"] = new("Telefon", "Azerbaijani text"),
            ["en"] = new("Phone", "English text")
        };

        Assert.Equal("Telefon", ProductTranslationResolver.Resolve(translations, "az-AZ,az;q=0.9").Translation.Name);
        Assert.Equal("Phone", ProductTranslationResolver.Resolve(translations, "ru").Translation.Name);
        Assert.Equal("Telefon", ProductTranslationResolver.Resolve(new Dictionary<string, ProductTranslation>
        {
            ["az"] = translations["az"]
        }, "ru").Translation.Name);
    }

    [Fact]
    public async Task CreateCategoryRejectsAnExistingGeneratedSlug()
    {
        await using var db = CreateDbContext();
        db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(), Name = "Electronics", Slug = SlugGenerator.Generate("Electronics"),
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
        var handler = new CategoryHandler(db);

        var result = await handler.CreateAsync(new CategoryRequest("Electronics", null), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Contains("slug", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);
}
