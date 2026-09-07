using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using ValidationException = Vitalis.Application.Exceptions.ValidationException;

namespace Vitalis.WebApi.Filters;

// Runs the FluentValidation validator matching each action argument's type (if one is
// registered) before the controller method executes, so controllers never call
// validators manually. Failures become a ValidationException, formatted by
// ExceptionHandlingMiddleware into the same ProblemDetails shape as any other error.
public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument));
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                throw new ValidationException(errors);
            }
        }

        await next();
    }
}
