namespace Vitalis.Application.Exceptions;

// Business-rule validation failures (e.g. "booking outside working hours").
// FluentValidation request validation (bước 3) is handled separately, before this ever runs.
public class ValidationException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }
}
