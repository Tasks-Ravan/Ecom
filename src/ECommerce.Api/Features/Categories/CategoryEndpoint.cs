using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using FluentValidation;

namespace ECommerce.Api.Features.Categories;

public sealed class CategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");
        group.MapGet("", async (CategoryHandler handler, CancellationToken ct) => Results.Ok(await handler.GetAllAsync(ct)))
            .WithName("GetCategories").WithSummary("List categories")
            .WithDescription("Returns categories as a flat list with parent IDs to represent the hierarchy. Example response: [{\"id\":\"...\",\"name\":\"Electronics\",\"slug\":\"electronics\",\"parentCategoryId\":null,\"isActive\":true}]")
            .Produces<IReadOnlyList<CategoryResponse>>();
        group.MapGet("/{id:guid}", async (Guid id, CategoryHandler handler, CancellationToken ct) =>
        {
            var category = await handler.GetByIdAsync(id, ct);
            return category is null ? Results.NotFound() : Results.Ok(category);
        }).WithName("GetCategoryById").WithSummary("Get a category")
          .WithDescription("Returns one category by its ID. Example response: {\"id\":\"...\",\"name\":\"Smartphones\",\"slug\":\"smartphones\",\"parentCategoryId\":\"...\",\"isActive\":true}.").Produces<CategoryResponse>().Produces(StatusCodes.Status404NotFound);
        group.MapPost("", async (CategoryRequest request, IValidator<CategoryRequest> validator, CategoryHandler handler, CancellationToken ct) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, ct);
            if (invalid is not null) return invalid;
            var result = await handler.CreateAsync(request, ct);
            return result.IsSuccess ? Results.Created($"/api/categories/{result.Value!.Id}", result.Value) : Results.Conflict(new { error = result.Error });
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("CreateCategory")
          .WithSummary("Create a category").WithDescription("Creates a category and derives a unique URL slug from its name. Example request: {\"name\":\"Accessories\",\"parentCategoryId\":null,\"isActive\":true}. Example response: {\"id\":\"...\",\"name\":\"Accessories\",\"slug\":\"accessories\",\"parentCategoryId\":null,\"isActive\":true}.")
          .Accepts<CategoryRequest>("application/json").Produces<CategoryResponse>(StatusCodes.Status201Created)
          .ProducesValidationProblem().Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{id:guid}", async (Guid id, CategoryRequest request, IValidator<CategoryRequest> validator,
            CategoryHandler handler, CancellationToken ct) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, ct);
            if (invalid is not null) return invalid;
            var result = await handler.UpdateAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : ResultResponse(result);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("UpdateCategory")
          .WithSummary("Update a category").WithDescription("Updates a category name, parent, and active state. Send the same JSON fields as CreateCategory; the response is the updated category including its generated slug.")
          .Accepts<CategoryRequest>("application/json").Produces<CategoryResponse>().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:guid}", async (Guid id, CategoryHandler handler, CancellationToken ct) =>
        {
            var result = await handler.DeleteAsync(id, ct);
            return result.IsSuccess ? Results.NoContent() : ResultResponse(result);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.Admin)).WithName("DeleteCategory")
          .WithSummary("Delete a category").WithDescription("Deletes an unused category without children or products.")
          .Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
    }

    private static IResult ResultResponse<T>(CatalogResult<T> result) => result.StatusCode switch
    {
        StatusCodes.Status404NotFound => Results.NotFound(new { error = result.Error }),
        StatusCodes.Status409Conflict => Results.Conflict(new { error = result.Error }),
        _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
    };
}
