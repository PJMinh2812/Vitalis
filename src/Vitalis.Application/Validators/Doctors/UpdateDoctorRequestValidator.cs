using FluentValidation;
using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Validators.Doctors;

public class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Họ tên không được để trống").MaximumLength(255);
        RuleFor(x => x.SpecialtyId).GreaterThan(0).WithMessage("Phải chọn chuyên khoa");
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0).WithMessage("Phí khám không được âm");
        RuleFor(x => x.Title).MaximumLength(50);
        RuleFor(x => x.LicenseNumber).MaximumLength(50);
        RuleFor(x => x.Room).MaximumLength(50);
    }
}
