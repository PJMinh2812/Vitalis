using FluentValidation;
using Vitalis.Application.DTOs.Billing;

namespace Vitalis.Application.Validators.Billing;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.Amount).NotEqual(0).WithMessage("Số tiền không được bằng 0");
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.ReferenceCode).MaximumLength(100);
        RuleFor(x => x.Note).MaximumLength(255);
    }
}
