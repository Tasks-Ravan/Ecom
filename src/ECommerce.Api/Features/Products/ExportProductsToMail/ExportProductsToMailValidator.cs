using FluentValidation;

namespace ECommerce.Api.Features.Products.ExportProductsToMail;

public sealed class ExportProductsToMailValidator : AbstractValidator<ExportProductsToMailRequest>
{
    public ExportProductsToMailValidator() =>
        RuleFor(request => request.RecipientEmail).NotEmpty().EmailAddress().MaximumLength(256);
}
