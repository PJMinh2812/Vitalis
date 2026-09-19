using FluentValidation;
using Vitalis.Application.DTOs.MedicalRecords;

namespace Vitalis.Application.Validators.MedicalRecords;

public class OrderServiceRequestValidator : AbstractValidator<OrderServiceRequest>
{
    public OrderServiceRequestValidator()
    {
        RuleFor(x => x.ServiceId).GreaterThan(0).WithMessage("Phải chọn dịch vụ");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
    }
}
