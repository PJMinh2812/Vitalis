using FluentValidation;
using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Validators.Doctors;

public class ScheduleRequestValidator : AbstractValidator<ScheduleRequest>
{
    public ScheduleRequestValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0).WithMessage("Phải chọn bác sĩ");
        RuleFor(x => x.DayOfWeek).InclusiveBetween((byte)0, (byte)6).WithMessage("Thứ không hợp lệ (0-6)");
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage("Giờ kết thúc phải sau giờ bắt đầu");
        RuleFor(x => x.SlotMinutes).InclusiveBetween(5, 120).WithMessage("Thời lượng slot phải từ 5 đến 120 phút");

        RuleFor(x => x.BreakEnd)
            .GreaterThan(x => x.BreakStart!.Value).WithMessage("Giờ nghỉ trưa kết thúc phải sau giờ bắt đầu")
            .When(x => x.BreakStart is not null && x.BreakEnd is not null);

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom).WithMessage("Ngày hết hiệu lực phải sau ngày bắt đầu hiệu lực")
            .When(x => x.EffectiveTo is not null);
    }
}
