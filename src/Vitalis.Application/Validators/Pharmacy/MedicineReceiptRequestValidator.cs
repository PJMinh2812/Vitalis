using FluentValidation;
using Vitalis.Application.DTOs.Pharmacy;

namespace Vitalis.Application.Validators.Pharmacy;

public class ReceiptItemRequestValidator : AbstractValidator<ReceiptItemRequest>
{
    public ReceiptItemRequestValidator()
    {
        RuleFor(x => x.MedicineId).GreaterThan(0);
        RuleFor(x => x.BatchNo).NotEmpty().WithMessage("Vui lòng nhập số lô").MaximumLength(50);
        RuleFor(x => x.ExpiryDate).GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Hạn dùng phải ở tương lai");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
        RuleFor(x => x.ImportPrice).GreaterThanOrEqualTo(0);
    }
}

public class MedicineReceiptRequestValidator : AbstractValidator<MedicineReceiptRequest>
{
    public MedicineReceiptRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Phiếu nhập phải có ít nhất một dòng");
        RuleForEach(x => x.Items).SetValidator(new ReceiptItemRequestValidator());
    }
}
