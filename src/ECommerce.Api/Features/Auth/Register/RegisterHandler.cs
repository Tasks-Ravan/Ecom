using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Features.Auth.Register;

public sealed class RegisterHandler(
    UserManager<ApplicationUser> userManager,
    ECommerce.Api.Infrastructure.IEmailSender emailSender,
    ILogger<RegisterHandler> logger)
{
    public async Task<Result<RegisterResponse>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = request.Email.Trim(), Email = request.Email.Trim(),
            FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim()
        };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(error => error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)))
                return Result<RegisterResponse>.Failure("An account with this email already exists.");
            return Result<RegisterResponse>.Failure(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
        var roleResult = await userManager.AddToRoleAsync(user, UserRole.Customer);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new InvalidOperationException("Could not assign the customer role to the new account.");
        }
        try
        {
            await emailSender.SendAsync(new EmailMessage(
                user.Email!,
                "Welcome to E-Commerce",
                $"Welcome, {user.FirstName}! Your account is ready.",
                $"<p>Welcome, {System.Net.WebUtility.HtmlEncode(user.FirstName)}! Your account is ready.</p>"), cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to send a welcome email to {Email}; registration succeeded.", user.Email);
        }
        return Result<RegisterResponse>.Success(new RegisterResponse(user.Id, user.FirstName, user.LastName, user.Email!));
    }
}
