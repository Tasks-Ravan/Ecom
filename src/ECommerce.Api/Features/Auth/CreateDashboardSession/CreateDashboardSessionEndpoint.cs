using System.IdentityModel.Tokens.Jwt;
using ECommerce.Api.Common;
using ECommerce.Api.Domain;

namespace ECommerce.Api.Features.Auth.CreateDashboardSession;

public sealed class CreateDashboardSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/dashboard-session", (HttpContext context) =>
        {
            var authorization = context.Request.Headers.Authorization.ToString();
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var accessToken = authorization["Bearer ".Length..].Trim();
            DateTimeOffset expiresAt;
            try
            {
                expiresAt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).ValidTo;
            }
            catch (ArgumentException)
            {
                return Results.Unauthorized();
            }

            var maxAge = expiresAt - DateTime.UtcNow;
            if (maxAge <= TimeSpan.Zero) return Results.Unauthorized();

            context.Response.Cookies.Append("hangfire_access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Secure = context.Request.IsHttps || context.Request.Headers["X-Forwarded-Proto"] == "https",
                Path = "/hangfire",
                MaxAge = maxAge
            });
            context.Response.Headers.CacheControl = "no-store";
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole(UserRole.Admin))
        .WithName("CreateHangfireDashboardSession")
        .WithTags("Authentication")
        .WithSummary("Open the Hangfire dashboard as an administrator")
        .WithDescription("Exchanges the authenticated administrator's current access token for an HttpOnly cookie scoped only to the Hangfire dashboard.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
