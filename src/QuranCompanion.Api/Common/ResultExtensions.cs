using Microsoft.AspNetCore.Mvc;
using QuranCompanion.Application.Common.Errors;
using QuranCompanion.Application.Common.Models;

namespace QuranCompanion.Api.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(ApiResponse<T>.Ok(result.Value));
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(
                ApiResponse<T>.Fail(result.Error.Message,
                    [new ApiError(result.Error.Code, result.Error.Message)])),
            ErrorType.Validation => new BadRequestObjectResult(
                ApiResponse<T>.Fail(result.Error.Message,
                    [new ApiError(result.Error.Code, result.Error.Message)])),
            ErrorType.Conflict => new ConflictObjectResult(
                ApiResponse<T>.Fail(result.Error.Message,
                    [new ApiError(result.Error.Code, result.Error.Message)])),
            _ => new ObjectResult(
                ApiResponse<T>.Fail(result.Error.Message,
                    [new ApiError(result.Error.Code, result.Error.Message)]))
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            },
        };
    }
}
