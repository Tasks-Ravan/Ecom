using FluentValidation;

namespace ECommerce.Api.Features.Brands;

public sealed class BrandValidator : AbstractValidator<BrandRequest>
{
    public BrandValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.LogoUrl).MaximumLength(2048).Must(value =>
            string.IsNullOrWhiteSpace(value) || (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)))
            .WithMessage("LogoUrl must be an absolute HTTP or HTTPS URL.");
    }
}
