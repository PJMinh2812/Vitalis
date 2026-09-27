using FluentValidation;
using Vitalis.Application.DTOs.Medicines;

namespace Vitalis.Application.Validators.Medicines;

public class CreateMedicineRequestValidator : AbstractValidator<CreateMedicineRequest>
{
    public CreateMedicineRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên thuốc không được để trống").MaximumLength(255);
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.ActiveIngredient).MaximumLength(255);
        RuleFor(x => x.Concentration).MaximumLength(100);
        RuleFor(x => x.Unit).MaximumLength(50);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Giá không được âm");
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Tồn kho không được âm");
    }
}
