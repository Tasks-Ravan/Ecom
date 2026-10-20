using System.Security.Claims;
using System.Security.Cryptography;
using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Infrastructure;
using ECommerce.Api.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Features.Auth.GoogleLogin;

public sealed class GoogleLoginHandler(
    UserManager<ApplicationUser> userManager,
    AppDbContext db,
    IEmailSender emailSender,
    ILogger<GoogleLoginHandler> logger)
{
    private static readonly TimeSpan ExchangeCodeLifetime = TimeSpan.FromMinutes(2);

    public async Task<Result<string>> CreateCodeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email)?.Trim();
        var emailVerified = principal.FindFirstValue("email_verified");
        if (string.IsNullOrWhiteSpace(providerKey) || string.IsNullOrWhiteSpace(email) ||
            !string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
            return Result<string>.Failure("Google did not provide a verified email address.");

        var user = await userManager.FindByLoginAsync("Google", providerKey);
        if (user is null)
        {
            user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                var userInfo = principal.FindFirstValue(ClaimTypes.Name)?.Trim();
                var userParts = (userInfo ?? string.Empty).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = principal.FindFirstValue(ClaimTypes.GivenName)?.Trim() ?? userParts.FirstOrDefault() ?? "Google",
                    LastName = principal.FindFirstValue(ClaimTypes.Surname)?.Trim() ?? (userParts.Length > 1 ? userParts[1] : "User")
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return Result<string>.Failure("The Google account could not be created.");

                var roleResult = await userManager.AddToRoleAsync(user, UserRole.Customer);
                if (!roleResult.Succeeded)
                {
                    await userManager.DeleteAsync(user);
                    return Result<string>.Failure("The customer role could not be assigned.");
                }

                try
                {
                    var firstName = System.Net.WebUtility.HtmlEncode(user.FirstName);
                    await emailSender.SendAsync(new EmailMessage(email,
                        "Welcome to E-Commerce",
                        $"Welcome, {user.FirstName}! Your account is ready.",
                        $"<p>Welcome, {firstName}! Your account is ready.</p>"), cancellationToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to send a welcome email to {Email}; Google sign-in succeeded.", email);
                }
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return Result<string>.Failure("The existing account could not be verified.");
            }

            var loginResult = await userManager.AddLoginAsync(user, new UserLoginInfo("Google", providerKey, "Google"));
            if (!loginResult.Succeeded)
                return Result<string>.Failure("The Google account could not be linked.");
        }

        var rawCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var codeHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawCode)));
        var now = DateTimeOffset.UtcNow;
        await db.GoogleOAuthCodes.Where(code => code.ExpiresAt <= now).ExecuteDeleteAsync(cancellationToken);
        db.GoogleOAuthCodes.Add(new GoogleOAuthCodeEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = codeHash,
            ExpiresAt = now.Add(ExchangeCodeLifetime)
        });
        await db.SaveChangesAsync(cancellationToken);
        return Result<string>.Success(rawCode);
    }

    public static string BuildFrontendCallbackUrl(string frontendBaseUrl) =>
        new Uri(new Uri(frontendBaseUrl.TrimEnd('/') + "/"), "auth/google/callback").ToString();

    public static string AddQuery(string callbackUrl, string name, string value)
    {
        var builder = new UriBuilder(callbackUrl) { Query = $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}" };
        return builder.Uri.ToString();
    }
}
