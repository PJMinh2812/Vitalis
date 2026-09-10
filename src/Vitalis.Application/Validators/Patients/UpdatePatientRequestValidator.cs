using Vitalis.Application.DTOs.Patients;

using FluentValidation;

namespace Vitalis.Application.Validators.Patients;

public class UpdatePatientRequestValidator : AbstractValidator<UpdatePatientRequest>
{
    public UpdatePatientRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(255);

        // Phone itself is optional here (see UpdatePatientRequest) — only its
        // format is checked when the caller actually sends one.
        RuleFor(x => x.Phone)
            .Matches(@"^0\d{9}$").WithMessage("Số điện thoại không đúng định dạng (10 số, bắt đầu bằng 0)")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .When(x => x.Email is not null);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Giới tính không hợp lệ");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Ngày sinh không được ở tương lai")
            .When(x => x.DateOfBirth is not null);

        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.NationalId).MaximumLength(20);
        RuleFor(x => x.InsuranceNumber).MaximumLength(20);
        RuleFor(x => x.BloodType).MaximumLength(5);
    }
}
