namespace ECommerce.Api.Features.Brands;

public sealed record BrandRequest(string Name, string? LogoUrl, bool IsActive = true);
public sealed record BrandResponse(Guid Id, string Name, string Slug, string? LogoUrl, bool IsActive,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
