using System.Security.Cryptography;
using System.Text;
using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Infrastructure;
using ECommerce.Api.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Features.Auth.GoogleLogin;

public sealed class GoogleLoginExchangeHandler(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IJwtTokenService tokens)
{
    public async Task<Result<Login.LoginResponse>> ExchangeAsync(
        GoogleLoginExchangeRequest request,
        CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Code)));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var stored = await db.GoogleOAuthCodes.AsNoTracking()
            .Where(code => code.CodeHash == hash && code.ExpiresAt > now)
            .Select(code => new { code.Id, code.UserId })
            .SingleOrDefaultAsync(cancellationToken);
        if (stored is null)
            return Result<Login.LoginResponse>.Failure("The Google login code is invalid or expired.");

        var consumed = await db.GoogleOAuthCodes
            .Where(code => code.Id == stored.Id && code.ExpiresAt > now)
            .ExecuteDeleteAsync(cancellationToken);
        if (consumed != 1)
            return Result<Login.LoginResponse>.Failure("The Google login code is invalid or expired.");

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
            return Result<Login.LoginResponse>.Failure("The Google account no longer exists.");

        var roles = await userManager.GetRolesAsync(user);
        var refreshToken = tokens.CreateRefreshToken();
        var refreshExpiry = tokens.RefreshTokenExpiresAt;
        db.RefreshTokens.Add(new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokens.HashRefreshToken(refreshToken),
            CreatedAt = now,
            ExpiresAt = refreshExpiry
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<Login.LoginResponse>.Success(new Login.LoginResponse(
            tokens.CreateAccessToken(user, roles), tokens.AccessTokenExpiresAt, refreshToken, refreshExpiry));
    }
}
