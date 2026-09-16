using FluentValidation;
using Vitalis.Application.DTOs.Appointments;

namespace Vitalis.Application.Validators.Appointments;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("Phải chọn bệnh nhân");
        RuleFor(x => x.DoctorId).GreaterThan(0).WithMessage("Phải chọn bác sĩ");
        RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow).WithMessage("Thời gian đặt lịch phải ở tương lai");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Vui lòng nhập lý do khám").MaximumLength(500);
        RuleFor(x => x.Source).IsInEnum();
    }
}
