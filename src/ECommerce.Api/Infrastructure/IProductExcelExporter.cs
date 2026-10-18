namespace ECommerce.Api.Infrastructure;

public interface IProductExcelExporter
{
    Task<byte[]> ExportAsync(CancellationToken cancellationToken);
}
