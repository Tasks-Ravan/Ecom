using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Infrastructure;
using ECommerce.Api.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenHandler(AppDbContext db, UserManager<ApplicationUser> userManager, IJwtTokenService tokens)
{
    public async Task<Result<Login.LoginResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var hash = tokens.HashRefreshToken(request.RefreshToken);
        var stored = await db.RefreshTokens.Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);
        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= DateTimeOffset.UtcNow)
            return Result<Login.LoginResponse>.Failure("The refresh token is invalid or expired.");

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var revoked = await db.RefreshTokens
            .Where(token => token.Id == stored.Id && token.RevokedAt == null && token.ExpiresAt > now)
            .ExecuteUpdateAsync(update => update.SetProperty(token => token.RevokedAt, now), cancellationToken);
        if (revoked != 1)
            return Result<Login.LoginResponse>.Failure("The refresh token is invalid or expired.");
        var user = stored.User;
        var refreshToken = tokens.CreateRefreshToken();
        var refreshExpiry = tokens.RefreshTokenExpiresAt;
        db.RefreshTokens.Add(new RefreshTokenEntity
        {
            Id = Guid.NewGuid(), UserId = user.Id, TokenHash = tokens.HashRefreshToken(refreshToken),
            CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = refreshExpiry
        });
        var roles = await userManager.GetRolesAsync(user);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result<Login.LoginResponse>.Success(new Login.LoginResponse(
            tokens.CreateAccessToken(user, roles), tokens.AccessTokenExpiresAt, refreshToken, refreshExpiry));
    }
}
