using System.ComponentModel.DataAnnotations;

namespace ECommerce.Api.Infrastructure;

public sealed class AdminOptions
{
    public const string SectionName = "Admin";
    [Required, EmailAddress] public string Email { get; init; } = string.Empty;
    [Required, MinLength(12)] public string Password { get; init; } = string.Empty;
    [Required] public string FirstName { get; init; } = string.Empty;
    [Required] public string LastName { get; init; } = string.Empty;
}
