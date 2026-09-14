using FluentValidation;
using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Validators.Doctors;

public class CreateSpecialtyRequestValidator : AbstractValidator<CreateSpecialtyRequest>
{
    public CreateSpecialtyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên chuyên khoa không được để trống").MaximumLength(255);
        RuleFor(x => x.Code).MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
