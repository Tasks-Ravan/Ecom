using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Api.Features.Categories;

public sealed class CategoryHandler(AppDbContext db)
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Categories.AsNoTracking().OrderBy(category => category.Name)
            .Select(category => new CategoryResponse(category.Id, category.Name, category.Slug,
                category.ParentCategoryId, category.IsActive, category.CreatedAt, category.UpdatedAt))
            .ToListAsync(cancellationToken);

    public async Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Categories.AsNoTracking().Where(category => category.Id == id)
            .Select(category => new CategoryResponse(category.Id, category.Name, category.Slug,
                category.ParentCategoryId, category.IsActive, category.CreatedAt, category.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<CatalogResult<CategoryResponse>> CreateAsync(CategoryRequest request, CancellationToken cancellationToken)
    {
        var slug = SlugGenerator.Generate(request.Name);
        if (await db.Categories.AnyAsync(category => category.Slug == slug, cancellationToken))
            return CatalogResult<CategoryResponse>.Conflict("A category with this slug already exists.");
        var now = DateTimeOffset.UtcNow;
        var category = new Category { Id = Guid.NewGuid(), Name = request.Name.Trim(), Slug = slug,
            ParentCategoryId = request.ParentCategoryId, IsActive = request.IsActive, CreatedAt = now, UpdatedAt = now };
        db.Categories.Add(category);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return CatalogResult<CategoryResponse>.Conflict("A category with this slug already exists."); }
        return CatalogResult<CategoryResponse>.Created(ToResponse(category));
    }

    public async Task<CatalogResult<CategoryResponse>> UpdateAsync(Guid id, CategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await db.Categories.SingleOrDefaultAsync(category => category.Id == id, cancellationToken);
        if (category is null) return CatalogResult<CategoryResponse>.NotFound("Category was not found.");
        if (request.ParentCategoryId == id || await WouldCreateCycleAsync(id, request.ParentCategoryId, cancellationToken))
            return CatalogResult<CategoryResponse>.Conflict("A category cannot be its own ancestor.");
        var slug = SlugGenerator.Generate(request.Name);
        if (await db.Categories.AnyAsync(other => other.Id != id && other.Slug == slug, cancellationToken))
            return CatalogResult<CategoryResponse>.Conflict("A category with this slug already exists.");
        category.Name = request.Name.Trim(); category.Slug = slug;
        category.ParentCategoryId = request.ParentCategoryId; category.IsActive = request.IsActive;
        category.UpdatedAt = DateTimeOffset.UtcNow;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return CatalogResult<CategoryResponse>.Conflict("A category with this slug already exists."); }
        return CatalogResult<CategoryResponse>.Ok(ToResponse(category));
    }

    public async Task<CatalogResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.SingleOrDefaultAsync(category => category.Id == id, cancellationToken);
        if (category is null) return CatalogResult<bool>.NotFound("Category was not found.");
        if (await db.Categories.AnyAsync(child => child.ParentCategoryId == id, cancellationToken) ||
            await db.Products.AnyAsync(product => product.CategoryId == id, cancellationToken))
            return CatalogResult<bool>.Conflict("Category has child categories or products and cannot be deleted.");
        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return CatalogResult<bool>.Ok(true);
    }

    private async Task<bool> WouldCreateCycleAsync(Guid id, Guid? parentId, CancellationToken cancellationToken)
    {
        var seen = new HashSet<Guid>();
        while (parentId is Guid current && seen.Add(current))
        {
            if (current == id) return true;
            parentId = await db.Categories.Where(category => category.Id == current)
                .Select(category => category.ParentCategoryId).SingleOrDefaultAsync(cancellationToken);
        }
        return false;
    }

    private static CategoryResponse ToResponse(Category category) => new(category.Id, category.Name, category.Slug,
        category.ParentCategoryId, category.IsActive, category.CreatedAt, category.UpdatedAt);
}
