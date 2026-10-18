using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Infrastructure;
using ECommerce.Api.Jobs;

namespace ECommerce.Api.Features.Products.ExportProductsToMail;

public sealed class DownloadProductExcelEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/export/download", async (IProductExcelExporter exporter, CancellationToken cancellationToken) =>
        {
            var content = await exporter.ExportAsync(cancellationToken);
            return Results.File(content, ProductExcelMailJob.ExcelContentType, "products.xlsx");
        })
        .RequireAuthorization(policy => policy.RequireRole(UserRole.Admin))
        .WithName("DownloadProductExcel")
        .WithTags("Product exports")
        .WithSummary("Download the product Excel export")
        .WithDescription("Builds and returns the current styled Excel workbook containing product names in Azerbaijani, English, and Russian.")
        .Produces(StatusCodes.Status200OK, contentType: ProductExcelMailJob.ExcelContentType)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
