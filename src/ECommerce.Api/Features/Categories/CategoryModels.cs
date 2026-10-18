namespace ECommerce.Api.Features.Categories;

public sealed record CategoryRequest(string Name, Guid? ParentCategoryId, bool IsActive = true);
public sealed record CategoryResponse(Guid Id, string Name, string Slug, Guid? ParentCategoryId, bool IsActive,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
