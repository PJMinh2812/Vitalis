using FluentValidation;
using Vitalis.Application.DTOs.MedicalRecords;

namespace Vitalis.Application.Validators.MedicalRecords;

public class PrescriptionItemRequestValidator : AbstractValidator<PrescriptionItemRequest>
{
    public PrescriptionItemRequestValidator()
    {
        RuleFor(x => x.MedicineId).GreaterThan(0).WithMessage("Phải chọn thuốc");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
        RuleFor(x => x.Dosage).NotEmpty().WithMessage("Vui lòng nhập liều dùng").MaximumLength(255);
        RuleFor(x => x.Instruction).MaximumLength(255);
        RuleFor(x => x.DurationDays).GreaterThan(0).When(x => x.DurationDays is not null);
    }
}

public class PrescriptionRequestValidator : AbstractValidator<PrescriptionRequest>
{
    public PrescriptionRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Đơn thuốc phải có ít nhất một loại thuốc");
        RuleForEach(x => x.Items).SetValidator(new PrescriptionItemRequestValidator());
    }
}
