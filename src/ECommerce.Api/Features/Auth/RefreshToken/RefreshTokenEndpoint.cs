using ECommerce.Api.Common;
using FluentValidation;

namespace ECommerce.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh", async (RefreshTokenRequest request, IValidator<RefreshTokenRequest> validator,
            RefreshTokenHandler handler, CancellationToken cancellationToken) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, cancellationToken);
            if (invalid is not null) return invalid;
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Unauthorized();
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh an access token")
        .WithDescription("Rotates a valid refresh token and returns a new access and refresh token pair.")
        .Accepts<RefreshTokenRequest>("application/json")
        .Produces<Login.LoginResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
