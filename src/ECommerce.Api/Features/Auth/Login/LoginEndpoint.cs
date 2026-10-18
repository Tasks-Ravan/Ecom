using ECommerce.Api.Common;
using FluentValidation;

namespace ECommerce.Api.Features.Auth.Login;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request, IValidator<LoginRequest> validator,
            LoginHandler handler, CancellationToken cancellationToken) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, cancellationToken);
            if (invalid is not null) return invalid;
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Unauthorized();
        })
        .WithName("Login")
        .WithSummary("Authenticate a customer")
        .WithDescription("Verifies credentials and returns a JWT access token and rotating refresh token.")
        .Accepts<LoginRequest>("application/json")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
