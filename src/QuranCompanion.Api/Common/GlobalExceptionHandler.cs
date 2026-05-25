using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuranCompanion.Application.Common.Exceptions;

namespace QuranCompanion.Api.Common;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validationException:
                {
                    var errors = validationException.Failures
                        .SelectMany(kvp => kvp.Value
                            .Select(msg => new ApiError("validation_error", msg, kvp.Key)))
                        .ToList();

                    var problem = new ProblemDetails
                    {
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Title = "Validation failed",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "One or more validation errors occurred.",
                    };

                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await httpContext.Response.WriteAsJsonAsync(
                        ApiResponse<object>.Fail(problem.Title, errors),
                        cancellationToken);
                    return true;
                }

            default:
                {
                    logger.LogError(exception, "Unhandled exception while processing request {Path}",
                        httpContext.Request.Path);

                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await httpContext.Response.WriteAsJsonAsync(
                        ApiResponse<object>.Fail(
                            "An unexpected error occurred.",
                            [new ApiError("unexpected_error", "An unexpected error occurred.")]),
                        cancellationToken);
                    return true;
                }
        }
    }
}
