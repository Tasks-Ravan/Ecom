using ECommerce.Api.Domain;

namespace ECommerce.Api.Infrastructure;

public interface IJwtTokenService
{
    string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    string CreateRefreshToken();
    string HashRefreshToken(string token);
    DateTimeOffset AccessTokenExpiresAt { get; }
    DateTimeOffset RefreshTokenExpiresAt { get; }
}
