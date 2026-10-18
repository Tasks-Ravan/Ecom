using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerce.Api.Persistence;

namespace ECommerce.Api.Features.Auth.Login;

public sealed class LoginHandler(UserManager<ApplicationUser> userManager, AppDbContext db, IJwtTokenService tokens)
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();
        var user = await userManager.Users.SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Result<LoginResponse>.Failure("Invalid email or password.");
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokens.CreateAccessToken(user, roles);
        var refreshToken = tokens.CreateRefreshToken();
        var accessExpiry = tokens.AccessTokenExpiresAt;
        var refreshExpiry = tokens.RefreshTokenExpiresAt;
        db.RefreshTokens.Add(new RefreshTokenEntity
        {
            Id = Guid.NewGuid(), UserId = user.Id, TokenHash = tokens.HashRefreshToken(refreshToken),
            CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = refreshExpiry
        });
        await db.SaveChangesAsync(cancellationToken);
        return Result<LoginResponse>.Success(new LoginResponse(accessToken, accessExpiry, refreshToken, refreshExpiry));
    }
}
