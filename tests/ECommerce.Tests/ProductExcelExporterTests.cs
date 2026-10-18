using ClosedXML.Excel;
using ECommerce.Api.Domain;
using ECommerce.Api.Infrastructure;
using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.Tests;

public sealed class ProductExcelExporterTests
{
    [Fact]
    public async Task ExportAsync_ProducesStyledWorkbookWithLocalizedProductData()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AppDbContext(options);
        var category = new Category { Id = Guid.NewGuid(), Name = "Phones", Slug = "phones" };
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Acme", Slug = "acme" };
        db.AddRange(category, brand);
        db.Products.Add(new Product
        {
            Id = Guid.NewGuid(), Sku = "PHONE-001", Category = category, Brand = brand,
            Price = 199.99m, StockQuantity = 7, IsActive = true,
            Translations = new Dictionary<string, ProductTranslation>
            {
                ["az"] = new("Telefon", " təsvir"),
                ["en"] = new("Phone", "Description"),
                ["ru"] = new("Телефон", "Описание")
            },
            CreatedAt = DateTimeOffset.Parse("2026-10-18T08:00:00Z")
        });
        await db.SaveChangesAsync();

        var bytes = await new ProductExcelExporter(db).ExportAsync(CancellationToken.None);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheet = workbook.Worksheet("Products");

        Assert.Equal("Sku", sheet.Cell(1, 1).GetString());
        Assert.Equal("Name (az)", sheet.Cell(1, 2).GetString());
        Assert.Equal("Name (en)", sheet.Cell(1, 3).GetString());
        Assert.Equal("Name (ru)", sheet.Cell(1, 4).GetString());
        Assert.True(sheet.Cell(1, 1).Style.Font.Bold);
        Assert.Equal("PHONE-001", sheet.Cell(2, 1).GetString());
        Assert.Equal("Telefon", sheet.Cell(2, 2).GetString());
        Assert.Equal("Phone", sheet.Cell(2, 3).GetString());
        Assert.Equal("Телефон", sheet.Cell(2, 4).GetString());
        Assert.Equal("Phones", sheet.Cell(2, 5).GetString());
        Assert.Equal("Acme", sheet.Cell(2, 6).GetString());
        Assert.Equal(199.99, sheet.Cell(2, 7).GetDouble());
        Assert.Equal(7, sheet.Cell(2, 8).GetValue<int>());
        Assert.True(sheet.Cell(2, 9).GetValue<bool>());
    }
}
