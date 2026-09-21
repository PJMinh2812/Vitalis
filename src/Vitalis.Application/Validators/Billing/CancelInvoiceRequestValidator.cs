using FluentValidation;
using Vitalis.Application.DTOs.Billing;

namespace Vitalis.Application.Validators.Billing;

public class CancelInvoiceRequestValidator : AbstractValidator<CancelInvoiceRequest>
{
    public CancelInvoiceRequestValidator()
    {
        RuleFor(x => x.CancelReason).NotEmpty().WithMessage("Vui lòng nhập lý do huỷ").MaximumLength(500);
    }
}
