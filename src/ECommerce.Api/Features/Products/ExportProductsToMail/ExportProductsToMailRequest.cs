namespace ECommerce.Api.Features.Products.ExportProductsToMail;

public sealed record ExportProductsToMailRequest(string RecipientEmail);
public sealed record ExportProductsToMailResponse(string JobId, string Status);
