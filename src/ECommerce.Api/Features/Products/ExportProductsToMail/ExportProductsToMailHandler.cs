using ECommerce.Api.Jobs;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace ECommerce.Api.Features.Products.ExportProductsToMail;

public sealed class ExportProductsToMailHandler(IBackgroundJobClient backgroundJobs)
{
    public string Enqueue(string recipientEmail)
    {
        var job = Job.FromExpression<ProductExcelMailJob>(
            handler => handler.RunAsync(recipientEmail, CancellationToken.None));
        return backgroundJobs.Create(job, new EnqueuedState());
    }
}
