using ECommerce.Api.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Features.Products;

public sealed class ProductValidator : AbstractValidator<ProductRequest>
{
    private static readonly HashSet<string> SupportedLanguages = ["az", "en", "ru"];

    public ProductValidator(AppDbContext db)
    {
        RuleFor(request => request.Sku).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0);
        RuleFor(request => request.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(request => request.CategoryId).MustAsync((id, cancellationToken) =>
            db.Categories.AnyAsync(category => category.Id == id, cancellationToken))
            .WithMessage("Category does not exist.");
        RuleFor(request => request.BrandId).MustAsync((id, cancellationToken) =>
            db.Brands.AnyAsync(brand => brand.Id == id, cancellationToken))
            .WithMessage("Brand does not exist.");
        RuleFor(request => request.Translations).NotNull().Must(translations => translations is { Count: > 0 })
            .WithMessage("At least one translation is required.");
        RuleForEach(request => request.Translations).ChildRules(translation =>
        {
            translation.RuleFor(entry => entry.Key).Must(key => SupportedLanguages.Contains(key.ToLowerInvariant()))
                .WithMessage("Translation language must be az, en, or ru.");
            translation.RuleFor(entry => entry.Value.Name).NotEmpty().MaximumLength(200);
            translation.RuleFor(entry => entry.Value.Description).NotEmpty().MaximumLength(5000);
        });
    }
}

public sealed class ProductQueryValidator : AbstractValidator<ProductQuery>
{
    public ProductQueryValidator()
    {
        RuleFor(query => query.Page).InclusiveBetween(1, 1_000_000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.MinPrice).GreaterThanOrEqualTo(0).When(query => query.MinPrice.HasValue);
        RuleFor(query => query.MaxPrice).GreaterThanOrEqualTo(0).When(query => query.MaxPrice.HasValue);
        RuleFor(query => query).Must(query => !query.MinPrice.HasValue || !query.MaxPrice.HasValue || query.MinPrice <= query.MaxPrice)
            .WithMessage("Minimum price must not exceed maximum price.");
        RuleFor(query => query.Sort).Must(sort => string.IsNullOrWhiteSpace(sort) ||
            new[] { "price_asc", "price_desc", "created_desc" }.Contains(sort, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Sort must be price_asc, price_desc, or created_desc.");
    }
}
