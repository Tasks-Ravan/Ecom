using ECommerce.Api.Common;
using FluentValidation;

namespace ECommerce.Api.Features.Auth.Register;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterRequest request, IValidator<RegisterRequest> validator,
            RegisterHandler handler, CancellationToken cancellationToken) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, cancellationToken);
            if (invalid is not null) return invalid;
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.IsSuccess ? Results.Created($"/api/users/{result.Value!.UserId}", result.Value)
                : Results.Conflict(new { error = result.Error });
        })
        .WithName("Register")
        .WithSummary("Register a customer account")
        .WithDescription("Creates a customer account using the supplied profile and password.")
        .Accepts<RegisterRequest>("application/json")
        .Produces<RegisterResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);
    }
}
