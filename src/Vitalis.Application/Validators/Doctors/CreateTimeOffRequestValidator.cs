using FluentValidation;
using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Validators.Doctors;

public class CreateTimeOffRequestValidator : AbstractValidator<CreateTimeOffRequest>
{
    public CreateTimeOffRequestValidator()
    {
        RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Loại nghỉ không hợp lệ");
        RuleFor(x => x.Reason).MaximumLength(255);
    }
}
