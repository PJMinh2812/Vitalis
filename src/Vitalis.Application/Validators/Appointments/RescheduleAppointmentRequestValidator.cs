using FluentValidation;
using Vitalis.Application.DTOs.Appointments;

namespace Vitalis.Application.Validators.Appointments;

public class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
{
    public RescheduleAppointmentRequestValidator()
    {
        RuleFor(x => x.NewStartTime).GreaterThan(DateTime.UtcNow).WithMessage("Thời gian mới phải ở tương lai");
    }
}
