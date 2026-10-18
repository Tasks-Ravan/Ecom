using ClosedXML.Excel;
using ECommerce.Api.Domain;
using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Infrastructure;

public sealed class ProductExcelExporter(AppDbContext db) : IProductExcelExporter
{
    private sealed record ExportRow(string Sku, Dictionary<string, ProductTranslation> Translations,
        string Category, string Brand, decimal Price, int Stock, bool IsActive, DateTimeOffset CreatedAt);

    private static readonly string[] Headers =
    ["Sku", "Name (az)", "Name (en)", "Name (ru)", "Category", "Brand", "Price", "Stock", "IsActive", "CreatedAt"];

    public async Task<byte[]> ExportAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Products.AsNoTracking()
            .OrderBy(product => product.Sku)
            .Select(product => new ExportRow(product.Sku, product.Translations, product.Category.Name,
                product.Brand.Name, product.Price, product.StockQuantity, product.IsActive, product.CreatedAt))
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Products");
        for (var column = 0; column < Headers.Length; column++)
            sheet.Cell(1, column + 1).Value = Headers[column];

        var header = sheet.Range(1, 1, 1, Headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#17365D");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var excelRow = index + 2;
            sheet.Cell(excelRow, 1).Value = row.Sku;
            sheet.Cell(excelRow, 2).Value = TranslationName(row.Translations, "az");
            sheet.Cell(excelRow, 3).Value = TranslationName(row.Translations, "en");
            sheet.Cell(excelRow, 4).Value = TranslationName(row.Translations, "ru");
            sheet.Cell(excelRow, 5).Value = row.Category;
            sheet.Cell(excelRow, 6).Value = row.Brand;
            sheet.Cell(excelRow, 7).Value = row.Price;
            sheet.Cell(excelRow, 8).Value = row.Stock;
            sheet.Cell(excelRow, 9).Value = row.IsActive;
            sheet.Cell(excelRow, 10).Value = row.CreatedAt.UtcDateTime;
            sheet.Cell(excelRow, 10).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
        }
        sheet.Columns(1, Headers.Length).AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string TranslationName(IReadOnlyDictionary<string, ProductTranslation> translations, string language) =>
        translations.FirstOrDefault(pair => string.Equals(pair.Key, language, StringComparison.OrdinalIgnoreCase)).Value?.Name ?? string.Empty;
}
