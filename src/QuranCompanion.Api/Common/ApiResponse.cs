namespace QuranCompanion.Api.Common;

public sealed record ApiResponse<T>(
    T? Data,
    bool Success,
    string? Message,
    IReadOnlyList<ApiError> Errors)
{
    public static ApiResponse<T> Ok(T data) =>
        new(data, true, null, Array.Empty<ApiError>());

    public static ApiResponse<T> Fail(string message, IReadOnlyList<ApiError>? errors = null) =>
        new(default, false, message, errors ?? Array.Empty<ApiError>());
}

public sealed record ApiError(string Code, string Message, string? Field = null);
