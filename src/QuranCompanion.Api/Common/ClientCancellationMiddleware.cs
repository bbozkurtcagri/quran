namespace QuranCompanion.Api.Common;

/// <summary>
/// Swallows <see cref="OperationCanceledException"/> caused by client aborts
/// (browser navigation, request timeout, React StrictMode double-effect, etc.)
/// and writes a 499 "Client Closed Request" status instead of letting the
/// exception propagate.
///
/// This sits ABOVE Serilog's request-logging middleware so the cancellation
/// never reaches it as a 500. It also sits above <see cref="ExceptionHandlerMiddleware"/>
/// because that middleware re-throws cancellation exceptions when
/// <c>RequestAborted</c> is signalled, bypassing any <c>IExceptionHandler</c>.
/// </summary>
public sealed class ClientCancellationMiddleware(
    RequestDelegate next,
    ILogger<ClientCancellationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug(
                "Request {Method} {Path} aborted by client",
                context.Request.Method,
                context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = 499; // Client Closed Request (nginx convention)
            }
        }
    }
}
