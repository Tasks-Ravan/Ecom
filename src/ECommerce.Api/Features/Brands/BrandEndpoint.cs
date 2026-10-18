using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using FluentValidation;

namespace ECommerce.Api.Features.Brands;

public sealed class BrandEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/brands").WithTags("Brands");
        group.MapGet("", async (BrandHandler handler, CancellationToken ct) => Results.Ok(await handler.GetAllAsync(ct)))
            .WithName("GetBrands").WithSummary("List brands").WithDescription("Returns all brands ordered by name. Example response: [{\"id\":\"...\",\"name\":\"Apple\",\"slug\":\"apple\",\"logoUrl\":null,\"isActive\":true}]")
            .Produces<IReadOnlyList<BrandResponse>>();
        group.MapGet("/{id:guid}", async (Guid id, BrandHandler handler, CancellationToken ct) =>
        {
            var brand = await handler.GetByIdAsync(id, ct);
            return brand is null ? Results.NotFound() : Results.Ok(brand);
        }).WithName("GetBrandById").WithSummary("Get a brand").WithDescription("Returns a brand by its ID. Example response: {\"id\":\"...\",\"name\":\"Apple\",\"slug\":\"apple\",\"logoUrl\":null,\"isActive\":true}.")
            .Produces<BrandResponse>().Produces(StatusCodes.Status404NotFound);
        group.MapPost("", async (BrandRequest request, IValidator<BrandRequest> validator, BrandHandler handler, CancellationToken ct) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, ct);
            if (invalid is not null) return invalid;
            var result = await handler.CreateAsync(request, ct);
            return result.IsSuccess ? Results.Created($"/api/brands/{result.Value!.Id}", result.Value) : Results.Conflict(new { error = result.Error });
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("CreateBrand")
            .WithSummary("Create a brand").WithDescription("Creates a brand with a generated unique slug. Example request: {\"name\":\"Contoso\",\"logoUrl\":null,\"isActive\":true}. Example response: {\"id\":\"...\",\"name\":\"Contoso\",\"slug\":\"contoso\",\"logoUrl\":null,\"isActive\":true}.")
            .Accepts<BrandRequest>("application/json").Produces<BrandResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem().Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{id:guid}", async (Guid id, BrandRequest request, IValidator<BrandRequest> validator, BrandHandler handler, CancellationToken ct) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, ct);
            if (invalid is not null) return invalid;
            var result = await handler.UpdateAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Error(result.StatusCode, result.Error);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("UpdateBrand")
            .WithSummary("Update a brand").WithDescription("Updates brand details and regenerates its slug from its name. Send the same JSON fields as CreateBrand; response is the updated brand.")
            .Accepts<BrandRequest>("application/json").Produces<BrandResponse>().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:guid}", async (Guid id, BrandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.DeleteAsync(id, ct);
            return result.IsSuccess ? Results.NoContent() : Error(result.StatusCode, result.Error);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("DeleteBrand")
            .WithSummary("Delete a brand").WithDescription("Deletes a brand only when it has no products.")
            .Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
    }

    private static IResult Error(int statusCode, string? error) => statusCode == StatusCodes.Status404NotFound
        ? Results.NotFound(new { error }) : Results.Conflict(new { error });
}
