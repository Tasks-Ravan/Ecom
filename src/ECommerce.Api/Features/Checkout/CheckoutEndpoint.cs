using System.Security.Claims;
using ECommerce.Api.Common;
using FluentValidation;

namespace ECommerce.Api.Features.Checkout;

public sealed class CheckoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}", async (Guid id, ClaimsPrincipal user, CheckoutHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Results.Unauthorized();
            var order = await handler.GetOrderAsync(userId, id, cancellationToken);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .RequireAuthorization()
        .WithName("GetMyOrder")
        .WithTags("Orders")
        .WithSummary("Get one of your orders")
        .WithDescription("Returns the saved order and item price snapshots only to the authenticated user who placed it.")
        .Produces<OrderDetailsResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/api/checkout", async (CheckoutRequest request, ClaimsPrincipal user,
            IValidator<CheckoutRequest> validator, CheckoutHandler handler, CancellationToken cancellationToken) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, cancellationToken);
            if (invalid is not null) return invalid;
            if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Results.Unauthorized();
            var result = await handler.HandleAsync(userId, request, cancellationToken);
            return result.IsSuccess ? Results.Created($"/api/orders/{result.Value!.OrderId}", result.Value) :
                result.StatusCode == StatusCodes.Status404NotFound ? Results.NotFound(new { error = result.Error }) :
                Results.Conflict(new { error = result.Error });
        })
        .RequireAuthorization()
        .WithName("SandboxCheckout")
        .WithTags("Checkout")
        .WithSummary("Complete a sandbox checkout")
        .WithDescription("Creates an order using server-side catalog prices and stock. Development-only: the sandbox can approve or decline a simulated payment; no card details or real payment provider are used. Enable with Payments:Sandbox:Enabled.")
        .Accepts<CheckoutRequest>("application/json")
        .Produces<CheckoutResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status404NotFound);
    }
}
