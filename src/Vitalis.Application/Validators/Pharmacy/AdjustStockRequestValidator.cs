using FluentValidation;
using Vitalis.Application.DTOs.Pharmacy;

namespace Vitalis.Application.Validators.Pharmacy;

public class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.BatchId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng điều chỉnh phải lớn hơn 0");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Vui lòng nhập lý do điều chỉnh").MaximumLength(255);
    }
}
