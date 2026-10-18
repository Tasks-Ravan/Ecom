using ECommerce.Api.Common;
using ECommerce.Api.Domain;

namespace ECommerce.Api.Features.Products;

public sealed record ProductRequest(string Sku, decimal Price, int StockQuantity, Guid CategoryId, Guid BrandId,
    bool IsActive, Dictionary<string, ProductTranslation> Translations);

public sealed record ProductResponse(Guid Id, string Sku, decimal Price, int StockQuantity, Guid CategoryId,
    string CategoryName, Guid BrandId, string BrandName, bool IsActive, string Language, string Name,
    string Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record ProductQuery(string? Search = null, Guid? CategoryId = null, Guid? BrandId = null,
    decimal? MinPrice = null, decimal? MaxPrice = null, int Page = 1, int PageSize = 20,
    string? Sort = null, string? Language = null);

public sealed record ProductListResponse(IReadOnlyList<ProductResponse> Items, int Page, int PageSize, int TotalCount);
