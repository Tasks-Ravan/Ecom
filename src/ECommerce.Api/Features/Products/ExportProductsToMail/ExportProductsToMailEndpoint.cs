using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using FluentValidation;

namespace ECommerce.Api.Features.Products.ExportProductsToMail;

public sealed class ExportProductsToMailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products/export/send-mail", async (ExportProductsToMailRequest request,
            IValidator<ExportProductsToMailRequest> validator, ExportProductsToMailHandler handler,
            CancellationToken cancellationToken) =>
        {
            var invalid = await ValidationFilter.ValidateAsync(request, validator, cancellationToken);
            if (invalid is not null) return invalid;
            var jobId = handler.Enqueue(request.RecipientEmail);
            return Results.Accepted($"/hangfire/jobs/details/{jobId}", new ExportProductsToMailResponse(jobId, "Enqueued"));
        })
        .RequireAuthorization(policy => policy.RequireRole(UserRole.Admin))
        .WithName("ExportProductsToMail")
        .WithTags("Product exports")
        .WithSummary("Email a product Excel export")
        .WithDescription("Enqueues a background job that generates the current product workbook and emails it as an attachment. Example request: {\"recipientEmail\":\"ops@example.com\"}. Example response: {\"jobId\":\"123\",\"status\":\"Enqueued\"}.")
        .Accepts<ExportProductsToMailRequest>("application/json")
        .Produces<ExportProductsToMailResponse>(StatusCodes.Status202Accepted)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
