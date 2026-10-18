using ECommerce.Api.Infrastructure;
using Hangfire;

namespace ECommerce.Api.Jobs;

public sealed class ProductExcelMailJob(
    IProductExcelExporter exporter,
    IEmailSender emailSender,
    ILogger<ProductExcelMailJob> logger)
{
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task RunAsync(string recipientEmail, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating product export for {Recipient}", recipientEmail);
        var content = await exporter.ExportAsync(cancellationToken);
        var attachment = new EmailAttachment(
            $"products-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx", ExcelContentType, content);
        await emailSender.SendAsync(new EmailMessage(
            recipientEmail,
            "E-Commerce product export",
            "The latest product export is attached.",
            "<p>The latest product export is attached.</p>",
            [attachment]), cancellationToken);
        logger.LogInformation("Product export sent to {Recipient}", recipientEmail);
    }
}
