using Vitalis.Application.DTOs.Patients;

using FluentValidation;

namespace Vitalis.Application.Validators.Patients;

public class QuickCreatePatientRequestValidator : AbstractValidator<QuickCreatePatientRequest>
{
    public QuickCreatePatientRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(255);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Số điện thoại không được để trống")
            .Matches(@"^0\d{9}$").WithMessage("Số điện thoại không đúng định dạng (10 số, bắt đầu bằng 0)");
    }
}
