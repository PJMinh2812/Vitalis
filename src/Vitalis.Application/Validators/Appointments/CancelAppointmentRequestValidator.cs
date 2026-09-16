using FluentValidation;
using Vitalis.Application.DTOs.Appointments;

namespace Vitalis.Application.Validators.Appointments;

public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.CancelReason).NotEmpty().WithMessage("Vui lòng nhập lý do huỷ").MaximumLength(500);
    }
}
