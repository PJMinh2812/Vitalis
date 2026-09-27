using FluentValidation;
using Vitalis.Application.DTOs.MedicalRecords;

namespace Vitalis.Application.Validators.MedicalRecords;

public class LabResultRequestValidator : AbstractValidator<LabResultRequest>
{
    public LabResultRequestValidator()
    {
        RuleFor(x => x.ResultValue).NotEmpty().WithMessage("Vui lòng nhập kết quả").MaximumLength(500);
        RuleFor(x => x.ReferenceRange).MaximumLength(255);
        RuleFor(x => x.Conclusion).MaximumLength(500);
    }
}
