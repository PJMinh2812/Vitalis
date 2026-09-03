using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.Exceptions;
using ValidationException = Vitalis.Application.Exceptions.ValidationException;

namespace Vitalis.WebApi.Middleware;

// Catches every unhandled exception and turns it into an RFC 7807 ProblemDetails
// response, so controllers never need their own try/catch for these cases.
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IProblemDetailsService problemDetailsService)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title) = MapException(ex);

            if (statusCode == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            else
                logger.LogWarning(ex, "{Title} on {Path}", title, context.Request.Path);

            context.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : ex.Message,
                Instance = context.Request.Path,
            };

            if (ex is ValidationException validationException)
                problemDetails.Extensions["errors"] = validationException.Errors;

            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problemDetails,
            });
        }
    }

    private static (int StatusCode, string Title) MapException(Exception ex) => ex switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
    };
}
