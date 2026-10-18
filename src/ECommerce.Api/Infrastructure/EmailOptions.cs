namespace ECommerce.Api.Infrastructure;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = "ECommerce";
    public bool UseStartTls { get; init; } = true;

    public bool IsLoggingMode => string.IsNullOrWhiteSpace(Host) || string.IsNullOrWhiteSpace(Password);

    public bool IsValidForSmtp => IsLoggingMode ||
        (!string.IsNullOrWhiteSpace(Username)
         && System.Net.Mail.MailAddress.TryCreate(FromEmail, out var parsedAddress)
         && string.Equals(parsedAddress.Address, FromEmail, StringComparison.OrdinalIgnoreCase)
         && Port is > 0 and <= 65535);
}
