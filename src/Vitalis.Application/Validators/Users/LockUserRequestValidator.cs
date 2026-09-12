using FluentValidation;
using Vitalis.Application.DTOs.Users;

namespace Vitalis.Application.Validators.Users;

public class LockUserRequestValidator : AbstractValidator<LockUserRequest>
{
    public LockUserRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Phải nhập lý do khoá tài khoản")
            .When(x => !x.IsActive);
    }
}
