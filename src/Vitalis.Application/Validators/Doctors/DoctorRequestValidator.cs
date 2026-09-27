using FluentValidation;
using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Validators.Doctors;

public class DoctorRequestValidator : AbstractValidator<DoctorRequest>
{
    public DoctorRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Họ tên không được để trống").MaximumLength(255);
        RuleFor(x => x.SpecialtyId).GreaterThan(0).WithMessage("Phải chọn chuyên khoa");
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0).WithMessage("Phí khám không được âm");
        RuleFor(x => x.Title).MaximumLength(50);
        RuleFor(x => x.LicenseNumber).MaximumLength(50);
        RuleFor(x => x.Room).MaximumLength(50);
        RuleFor(x => x.Username).NotEmpty().WithMessage("Cần đặt tên đăng nhập cho bác sĩ");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Mật khẩu tối thiểu 6 ký tự");
    }
}
