using FluentValidation;
using Vitalis.Application.DTOs.Users;

namespace Vitalis.Application.Validators.Users;

public class AssignRolesRequestValidator : AbstractValidator<AssignRolesRequest>
{
    public AssignRolesRequestValidator()
    {
        RuleFor(x => x.RoleIds).NotEmpty().WithMessage("Phải chọn ít nhất 1 vai trò");
    }
}
