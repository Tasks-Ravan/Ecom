using FluentValidation;

namespace ECommerce.Api.Features.Checkout;

public sealed class CheckoutValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutValidator()
    {
        RuleFor(request => request.Items).NotNull().NotEmpty().Must(items => items is { Count: <= 50 })
            .WithMessage("A checkout must contain between 1 and 50 distinct products.");
        RuleForEach(request => request.Items).ChildRules(item =>
        {
            item.RuleFor(line => line.ProductId).NotEmpty();
            item.RuleFor(line => line.Quantity).InclusiveBetween(1, 20);
        });
        RuleFor(request => request).Must(request => request.Items is not null &&
            request.Items.Select(item => item.ProductId).Distinct().Count() == request.Items.Count)
            .WithMessage("Each product may appear only once in the checkout.");
        RuleFor(request => request.Language).Must(language => new[] { "az", "en", "ru" }
            .Contains(language, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Language must be az, en, or ru.");
    }
}
