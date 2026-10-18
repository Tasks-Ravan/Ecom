using FluentValidation;

namespace ECommerce.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator() => RuleFor(request => request.RefreshToken).NotEmpty().MaximumLength(256);
}
