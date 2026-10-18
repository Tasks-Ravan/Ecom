using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using FluentValidation;

namespace ECommerce.Api.Features.Products;

public sealed class ProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");
        group.MapGet("", async (string? search, Guid? categoryId, Guid? brandId, decimal? minPrice, decimal? maxPrice,
            int? page, int? pageSize, string? sort, string? lang, HttpContext httpContext,
            IValidator<ProductQuery> validator, ProductHandler handler, CancellationToken ct) =>
        {
            var language = string.IsNullOrWhiteSpace(lang) ? httpContext.Request.Headers.AcceptLanguage.ToString() : lang;
            var query = new ProductQuery(search, categoryId, brandId, minPrice, maxPrice,
                page ?? 1, pageSize ?? 20, sort, language);
            var invalid = await ValidationFilter.ValidateAsync(query, validator, ct);
            if (invalid is not null) return invalid;
            return Results.Ok(await handler.GetAllAsync(query, ct));
        }).WithName("GetProducts").WithSummary("Search and list products")
          .WithDescription("Filters products by translated name, category, brand, and price; supports pagination and price sorting. Language comes from lang or Accept-Language, with English then first available translation as fallback. Example: /api/products?lang=az&search=telefon&minPrice=100&maxPrice=900&sort=price_desc&page=1&pageSize=20. Example response: {\"items\":[{\"sku\":\"ELC-PHN-0001\",\"language\":\"az\",\"name\":\"iPhone 15\",\"price\":899}],\"page\":1,\"pageSize\":20,\"totalCount\":24}.")
          .Produces<ProductListResponse>().ProducesValidationProblem();
        group.MapGet("/{id:guid}", async (Guid id, string? lang, HttpContext httpContext, ProductHandler handler, CancellationToken ct) =>
        {
            var language = string.IsNullOrWhiteSpace(lang) ? httpContext.Request.Headers.AcceptLanguage.ToString() : lang;
            var product = await handler.GetByIdAsync(id, language, ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        }).WithName("GetProductById").WithSummary("Get product details")
          .WithDescription("Returns localized product details, honoring lang first and Accept-Language second, with English then first-available fallback. Example response: {\"sku\":\"ELC-PHN-0001\",\"language\":\"az\",\"name\":\"iPhone 15\",\"description\":\"48 MP kameralı güclü smartfon.\"}.")
          .Produces<ProductResponse>().Produces(StatusCodes.Status404NotFound);
        group.MapPost("", async (ProductRequest request, IValidator<ProductRequest> validator,
            ProductHandler handler, CancellationToken ct) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, ct);
            if (invalid is not null) return invalid;
            var result = await handler.CreateAsync(request, ct);
            return result.IsSuccess ? Results.Created($"/api/products/{result.Value!.Id}", result.Value) : Results.Conflict(new { error = result.Error });
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("CreateProduct")
          .WithSummary("Create a product").WithDescription("Creates a product with translations stored together in PostgreSQL jsonb. Example request: { \"sku\": \"ELC-PHN-1001\", \"price\": 499.99, \"stockQuantity\": 10, \"categoryId\": \"00000000-0000-0000-0000-000000000001\", \"brandId\": \"00000000-0000-0000-0000-000000000002\", \"isActive\": true, \"translations\": { \"en\": { \"name\": \"Phone X\", \"description\": \"A capable smartphone.\" }, \"az\": { \"name\": \"Telefon X\", \"description\": \"Güclü smartfon.\" } } }")
          .Accepts<ProductRequest>("application/json").Produces<ProductResponse>(StatusCodes.Status201Created)
          .ProducesValidationProblem().Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{id:guid}", async (Guid id, ProductRequest request, IValidator<ProductRequest> validator,
            ProductHandler handler, CancellationToken ct) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, ct);
            if (invalid is not null) return invalid;
            var result = await handler.UpdateAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Error(result.StatusCode, result.Error);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("UpdateProduct")
          .WithSummary("Update a product").WithDescription("Replaces product fields and its complete translations dictionary. Send the same JSON request shape as CreateProduct; response is the updated product localized in English, or its first available translation if English is absent.")
          .Accepts<ProductRequest>("application/json").Produces<ProductResponse>().ProducesValidationProblem()
          .Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:guid}", async (Guid id, ProductHandler handler, CancellationToken ct) =>
        {
            var result = await handler.DeleteAsync(id, ct);
            return result.IsSuccess ? Results.NoContent() : Error(result.StatusCode, result.Error);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("DeleteProduct")
          .WithSummary("Delete a product").WithDescription("Permanently deletes a product by ID.")
          .Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
    }

    private static IResult Error(int statusCode, string? error) => statusCode == StatusCodes.Status404NotFound
        ? Results.NotFound(new { error }) : Results.Conflict(new { error });
}
