using System.ComponentModel.DataAnnotations;

namespace ECommerce.Api.Infrastructure;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required] public string Issuer { get; init; } = string.Empty;
    [Required] public string Audience { get; init; } = string.Empty;
    [Required] public string SecretKey { get; init; } = string.Empty;
    [Range(1, 1440)] public int ExpirationMinutes { get; init; } = 15;
}
