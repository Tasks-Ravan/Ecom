namespace ECommerce.Api.Domain;

public sealed class GoogleOAuthCodeEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}
