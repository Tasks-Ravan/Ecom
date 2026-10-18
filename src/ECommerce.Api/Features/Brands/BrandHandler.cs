using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Api.Features.Brands;

public sealed class BrandHandler(AppDbContext db)
{
    public async Task<IReadOnlyList<BrandResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Brands.AsNoTracking().OrderBy(brand => brand.Name)
            .Select(brand => new BrandResponse(brand.Id, brand.Name, brand.Slug, brand.LogoUrl,
                brand.IsActive, brand.CreatedAt, brand.UpdatedAt)).ToListAsync(cancellationToken);

    public async Task<BrandResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Brands.AsNoTracking().Where(brand => brand.Id == id)
            .Select(brand => new BrandResponse(brand.Id, brand.Name, brand.Slug, brand.LogoUrl,
                brand.IsActive, brand.CreatedAt, brand.UpdatedAt)).SingleOrDefaultAsync(cancellationToken);

    public async Task<CatalogResult<BrandResponse>> CreateAsync(BrandRequest request, CancellationToken cancellationToken)
    {
        var slug = SlugGenerator.Generate(request.Name);
        if (await db.Brands.AnyAsync(brand => brand.Slug == slug || brand.Name == request.Name.Trim(), cancellationToken))
            return CatalogResult<BrandResponse>.Conflict("A brand with this name or slug already exists.");
        var now = DateTimeOffset.UtcNow;
        var brand = new Brand { Id = Guid.NewGuid(), Name = request.Name.Trim(), Slug = slug,
            LogoUrl = request.LogoUrl, IsActive = request.IsActive, CreatedAt = now, UpdatedAt = now };
        db.Brands.Add(brand);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return CatalogResult<BrandResponse>.Conflict("A brand with this name or slug already exists."); }
        return CatalogResult<BrandResponse>.Created(ToResponse(brand));
    }

    public async Task<CatalogResult<BrandResponse>> UpdateAsync(Guid id, BrandRequest request, CancellationToken cancellationToken)
    {
        var brand = await db.Brands.SingleOrDefaultAsync(brand => brand.Id == id, cancellationToken);
        if (brand is null) return CatalogResult<BrandResponse>.NotFound("Brand was not found.");
        var slug = SlugGenerator.Generate(request.Name);
        if (await db.Brands.AnyAsync(other => other.Id != id && (other.Slug == slug || other.Name == request.Name.Trim()), cancellationToken))
            return CatalogResult<BrandResponse>.Conflict("A brand with this name or slug already exists.");
        brand.Name = request.Name.Trim(); brand.Slug = slug; brand.LogoUrl = request.LogoUrl;
        brand.IsActive = request.IsActive; brand.UpdatedAt = DateTimeOffset.UtcNow;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return CatalogResult<BrandResponse>.Conflict("A brand with this name or slug already exists."); }
        return CatalogResult<BrandResponse>.Ok(ToResponse(brand));
    }

    public async Task<CatalogResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var brand = await db.Brands.SingleOrDefaultAsync(brand => brand.Id == id, cancellationToken);
        if (brand is null) return CatalogResult<bool>.NotFound("Brand was not found.");
        if (await db.Products.AnyAsync(product => product.BrandId == id, cancellationToken))
            return CatalogResult<bool>.Conflict("Brand has products and cannot be deleted.");
        db.Brands.Remove(brand);
        await db.SaveChangesAsync(cancellationToken);
        return CatalogResult<bool>.Ok(true);
    }

    private static BrandResponse ToResponse(Brand brand) => new(brand.Id, brand.Name, brand.Slug,
        brand.LogoUrl, brand.IsActive, brand.CreatedAt, brand.UpdatedAt);
}
