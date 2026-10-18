using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Api.Features.Products;

public sealed class ProductHandler(AppDbContext db)
{
    public async Task<ProductListResponse> GetAllAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Product> products = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            products = db.Products.FromSqlInterpolated($"""
                SELECT p.* FROM "Products" AS p
                WHERE EXISTS (
                    SELECT 1 FROM jsonb_each(p.translations) AS translated(language, content)
                    WHERE translated.content->>'name' ILIKE {pattern})
                """).AsNoTracking();
        }
        if (query.CategoryId.HasValue) products = products.Where(product => product.CategoryId == query.CategoryId.Value);
        if (query.BrandId.HasValue) products = products.Where(product => product.BrandId == query.BrandId.Value);
        if (query.MinPrice.HasValue) products = products.Where(product => product.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue) products = products.Where(product => product.Price <= query.MaxPrice.Value);
        products = query.Sort?.ToLowerInvariant() switch
        {
            "price_asc" => products.OrderBy(product => product.Price).ThenBy(product => product.Id),
            "price_desc" => products.OrderByDescending(product => product.Price).ThenBy(product => product.Id),
            _ => products.OrderByDescending(product => product.CreatedAt).ThenBy(product => product.Id)
        };
        var totalCount = await products.CountAsync(cancellationToken);
        var page = await products.Include(product => product.Category).Include(product => product.Brand)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        return new ProductListResponse(page.Select(product => ToResponse(product, query.Language)).ToArray(),
            query.Page, query.PageSize, totalCount);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, string? language, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking().Include(product => product.Category).Include(product => product.Brand)
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        return product is null ? null : ToResponse(product, language);
    }

    public async Task<CatalogResult<ProductResponse>> CreateAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        if (await db.Products.AnyAsync(product => product.Sku == request.Sku.Trim(), cancellationToken))
            return CatalogResult<ProductResponse>.Conflict("A product with this SKU already exists.");
        if (!await ReferencesExistAsync(request, cancellationToken))
            return CatalogResult<ProductResponse>.Conflict("The selected category or brand no longer exists.");
        var now = DateTimeOffset.UtcNow;
        var product = new Product { Id = Guid.NewGuid(), Sku = request.Sku.Trim(), Price = request.Price,
            StockQuantity = request.StockQuantity, CategoryId = request.CategoryId, BrandId = request.BrandId,
            IsActive = request.IsActive, Translations = NormalizeTranslations(request.Translations), CreatedAt = now, UpdatedAt = now };
        db.Products.Add(product);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return CatalogResult<ProductResponse>.Conflict("A product with this SKU already exists."); }
        await db.Entry(product).Reference(value => value.Category).LoadAsync(cancellationToken);
        await db.Entry(product).Reference(value => value.Brand).LoadAsync(cancellationToken);
        return CatalogResult<ProductResponse>.Created(ToResponse(product, "en"));
    }

    public async Task<CatalogResult<ProductResponse>> UpdateAsync(Guid id, ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (product is null) return CatalogResult<ProductResponse>.NotFound("Product was not found.");
        if (!await ReferencesExistAsync(request, cancellationToken))
            return CatalogResult<ProductResponse>.Conflict("The selected category or brand no longer exists.");
        if (await db.Products.AnyAsync(other => other.Id != id && other.Sku == request.Sku.Trim(), cancellationToken))
            return CatalogResult<ProductResponse>.Conflict("A product with this SKU already exists.");
        product.Sku = request.Sku.Trim(); product.Price = request.Price; product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId; product.BrandId = request.BrandId; product.IsActive = request.IsActive;
        product.Translations = NormalizeTranslations(request.Translations); product.UpdatedAt = DateTimeOffset.UtcNow;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return CatalogResult<ProductResponse>.Conflict("A product with this SKU already exists."); }
        await db.Entry(product).Reference(value => value.Category).LoadAsync(cancellationToken);
        await db.Entry(product).Reference(value => value.Brand).LoadAsync(cancellationToken);
        return CatalogResult<ProductResponse>.Ok(ToResponse(product, "en"));
    }

    public async Task<CatalogResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (product is null) return CatalogResult<bool>.NotFound("Product was not found.");
        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return CatalogResult<bool>.Ok(true);
    }

    private async Task<bool> ReferencesExistAsync(ProductRequest request, CancellationToken cancellationToken) =>
        await db.Categories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken) &&
        await db.Brands.AnyAsync(brand => brand.Id == request.BrandId, cancellationToken);

    private static Dictionary<string, ProductTranslation> NormalizeTranslations(Dictionary<string, ProductTranslation> translations) =>
        translations.ToDictionary(pair => pair.Key.Trim().ToLowerInvariant(), pair => pair.Value, StringComparer.OrdinalIgnoreCase);

    private static ProductResponse ToResponse(Product product, string? requestedLanguage)
    {
        var (language, translation) = ProductTranslationResolver.Resolve(product.Translations, requestedLanguage);
        return new ProductResponse(product.Id, product.Sku, product.Price, product.StockQuantity, product.CategoryId,
            product.Category.Name, product.BrandId, product.Brand.Name, product.IsActive, language,
            translation.Name, translation.Description, product.CreatedAt, product.UpdatedAt);
    }
}
