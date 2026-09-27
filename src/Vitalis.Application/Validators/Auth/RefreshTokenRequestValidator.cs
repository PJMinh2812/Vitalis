using FluentValidation;
using Vitalis.Application.DTOs.Auth;

namespace Vitalis.Application.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Thiếu refresh token");
    }
}
