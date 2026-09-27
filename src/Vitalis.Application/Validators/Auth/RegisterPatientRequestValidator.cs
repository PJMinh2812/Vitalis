using FluentValidation;
using Vitalis.Application.DTOs.Auth;

namespace Vitalis.Application.Validators.Auth;

public class RegisterPatientRequestValidator : AbstractValidator<RegisterPatientRequest>
{
    public RegisterPatientRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(255);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Số điện thoại không được để trống")
            .Matches(@"^0\d{9}$").WithMessage("Số điện thoại không đúng định dạng (10 số, bắt đầu bằng 0)");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .When(x => x.Email is not null);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Ngày sinh không được ở tương lai")
            .When(x => x.DateOfBirth is not null);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự");
    }
}
