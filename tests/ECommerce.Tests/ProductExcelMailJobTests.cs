using ECommerce.Api.Infrastructure;
using ECommerce.Api.Jobs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ECommerce.Tests;

public sealed class ProductExcelMailJobTests
{
    [Fact]
    public async Task RunAsync_SendsGeneratedWorkbookAsAttachment()
    {
        var content = new byte[] { 1, 2, 3, 4 };
        var exporter = new Mock<IProductExcelExporter>();
        exporter.Setup(service => service.ExportAsync(It.IsAny<CancellationToken>())).ReturnsAsync(content);
        var emailSender = new Mock<IEmailSender>();
        emailSender.Setup(service => service.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var job = new ProductExcelMailJob(exporter.Object, emailSender.Object,
            NullLogger<ProductExcelMailJob>.Instance);

        await job.RunAsync("ops@example.com", CancellationToken.None);

        emailSender.Verify(service => service.SendAsync(
            It.Is<EmailMessage>(message => message.To == "ops@example.com"
                && message.Attachments != null && message.Attachments.Count == 1
                && message.Attachments[0].ContentType == ProductExcelMailJob.ExcelContentType
                && message.Attachments[0].FileName.EndsWith(".xlsx", StringComparison.Ordinal)
                && message.Attachments[0].Content.SequenceEqual(content)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
