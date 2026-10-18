using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ECommerce.Api.Infrastructure;

public sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (settings.IsLoggingMode)
            throw new InvalidOperationException("SMTP sender was selected without configured host and password.");

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;

        var body = new BodyBuilder
        {
            TextBody = message.PlainTextContent,
            HtmlBody = message.HtmlContent
        };
        foreach (var attachment in message.Attachments ?? [])
            body.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        mimeMessage.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(settings.Host, settings.Port, socketOptions, cancellationToken);
        await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
        await client.SendAsync(mimeMessage, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        logger.LogInformation("Email sent via SMTP to {Recipient} with subject {Subject}", message.To, message.Subject);
    }
}
