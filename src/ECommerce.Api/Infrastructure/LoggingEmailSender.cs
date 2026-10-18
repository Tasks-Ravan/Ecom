namespace ECommerce.Api.Infrastructure;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var attachments = message.Attachments is { Count: > 0 }
            ? string.Join(", ", message.Attachments.Select(attachment => $"{attachment.FileName} ({attachment.Content.Length} bytes)"))
            : "none";
        logger.LogInformation(
            "Email logging mode; no email sent. Recipient={Recipient}, Subject={Subject}, Attachments={Attachments}",
            message.To, message.Subject, attachments);
        return Task.CompletedTask;
    }
}
