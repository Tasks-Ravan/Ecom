using ECommerce.Api.Common;
using ECommerce.Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Features.Auth.GoogleLogin;

public sealed class GoogleLoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/google", (IOptions<GoogleOAuthOptions> options, IOptions<ClientAppOptions> clientOptions) =>
        {
            if (!options.Value.IsConfigured)
                return Results.Redirect(GoogleLoginHandler.AddQuery(
                    GoogleLoginHandler.BuildFrontendCallbackUrl(clientOptions.Value.BaseUrl),
                    "error", "google_not_configured"));

            var properties = new AuthenticationProperties { RedirectUri = "/api/auth/google/callback" };
            return Results.Challenge(properties, [Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme]);
        })
        .WithName("GoogleLogin")
        .WithSummary("Sign in with Google")
        .WithDescription("Starts Google's OAuth 2.0 authorization flow and returns to the configured frontend callback.")
        .Produces(StatusCodes.Status302Found)
        .AllowAnonymous();

        app.MapGet("/api/auth/google/callback", async (HttpContext context,
            GoogleLoginHandler handler, IOptions<ClientAppOptions> clientOptions,
            CancellationToken cancellationToken) =>
        {
            var external = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
            await context.SignOutAsync(IdentityConstants.ExternalScheme);
            var callbackUrl = GoogleLoginHandler.BuildFrontendCallbackUrl(clientOptions.Value.BaseUrl);
            if (!external.Succeeded || external.Principal is null)
                return Results.Redirect(GoogleLoginHandler.AddQuery(callbackUrl, "error", "google_login_failed"));

            var result = await handler.CreateCodeAsync(external.Principal, cancellationToken);
            return result.IsSuccess
                ? Results.Redirect(GoogleLoginHandler.AddQuery(callbackUrl, "code", result.Value!))
                : Results.Redirect(GoogleLoginHandler.AddQuery(callbackUrl, "error", "google_login_failed"));
        })
        .WithName("GoogleLoginCallback")
        .WithSummary("Complete Google sign-in")
        .WithDescription("Validates the Google identity and redirects to the frontend with a short-lived, single-use exchange code.")
        .Produces(StatusCodes.Status302Found)
        .AllowAnonymous();

        app.MapPost("/api/auth/google/exchange", async (GoogleLoginExchangeRequest request,
            IValidator<GoogleLoginExchangeRequest> validator, GoogleLoginExchangeHandler handler,
            CancellationToken cancellationToken) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, cancellationToken);
            if (invalid is not null) return invalid;
            var result = await handler.ExchangeAsync(request, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Unauthorized();
        })
        .WithName("GoogleLoginExchange")
        .WithSummary("Exchange Google login code")
        .WithDescription("Consumes a one-time Google login code and returns the application's access and refresh tokens.")
        .Accepts<GoogleLoginExchangeRequest>("application/json")
        .Produces<Login.LoginResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
