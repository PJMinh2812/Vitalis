using FluentValidation;
using Vitalis.Application.DTOs.MedicalRecords;

namespace Vitalis.Application.Validators.MedicalRecords;

public class MedicalRecordRequestValidator : AbstractValidator<MedicalRecordRequest>
{
    public MedicalRecordRequestValidator()
    {
        RuleFor(x => x.Symptoms).NotEmpty().WithMessage("Vui lòng nhập triệu chứng").MaximumLength(4000);
        RuleFor(x => x.Diagnosis).NotEmpty().WithMessage("Vui lòng nhập chẩn đoán").MaximumLength(4000);
        RuleFor(x => x.Icd10Code).MaximumLength(10);
        RuleFor(x => x.FollowUpDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Ngày tái khám không được ở quá khứ")
            .When(x => x.FollowUpDate is not null);
    }
}
