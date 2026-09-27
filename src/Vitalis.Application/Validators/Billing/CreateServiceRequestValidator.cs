using FluentValidation;
using Vitalis.Application.DTOs.Billing;

namespace Vitalis.Application.Validators.Billing;

public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên dịch vụ không được để trống").MaximumLength(255);
        RuleFor(x => x.Code).MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Giá không được âm");
        RuleFor(x => x.DurationMinutes).GreaterThan(0).When(x => x.DurationMinutes is not null);
    }
}
