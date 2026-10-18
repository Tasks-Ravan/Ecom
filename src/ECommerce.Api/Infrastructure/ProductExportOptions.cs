using System.ComponentModel.DataAnnotations;

namespace ECommerce.Api.Infrastructure;

public sealed class ProductExportOptions
{
    public const string SectionName = "ProductExport";

    [Required, EmailAddress]
    public string RecipientEmail { get; init; } = string.Empty;

    [Required]
    public string CronExpression { get; init; } = "0 8 * * *";
}
