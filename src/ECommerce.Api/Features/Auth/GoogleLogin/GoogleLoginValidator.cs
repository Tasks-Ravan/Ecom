using FluentValidation;

namespace ECommerce.Api.Features.Auth.GoogleLogin;

public sealed class GoogleLoginValidator : AbstractValidator<GoogleLoginExchangeRequest>
{
    public GoogleLoginValidator() => RuleFor(request => request.Code).NotEmpty().MaximumLength(128);
}
