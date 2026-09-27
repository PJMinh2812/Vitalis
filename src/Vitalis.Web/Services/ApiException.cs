namespace Vitalis.Web.Services;

public class ApiException(int statusCode, string message, IDictionary<string, string[]>? errors = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    // Field-level errors from ValidationException's ProblemDetails.Extensions["errors"] (see ExceptionHandlingMiddleware).
    public IDictionary<string, string[]>? Errors { get; } = errors;
}
