using System.Security.Cryptography;
using Microsoft.Net.Http.Headers;

namespace QuranCompanion.Api.Common;

/// <summary>
/// Adds an <c>ETag</c> header to successful GET responses and short-circuits
/// to 304 Not Modified when the client's <c>If-None-Match</c> matches.
///
/// The ETag is computed as the first 16 hex chars of the SHA-256 of the
/// response body — strong enough for collision resistance at our scale,
/// cheap enough to compute per request. Buffering is opt-in: we only buffer
/// when the path looks cacheable so search and health checks pay no cost.
/// </summary>
public sealed class ETagMiddleware(RequestDelegate next)
{
    private static readonly string[] CacheablePathPrefixes =
    [
        "/api/v1/surahs",
        "/api/v1/verses",
        "/api/v1/translation-sources",
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldHandle(context))
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);

            if (context.Response.StatusCode != StatusCodes.Status200OK
                || buffer.Length == 0)
            {
                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody);
                return;
            }

            var etag = ComputeETag(buffer);
            context.Response.Headers[HeaderNames.ETag] = etag;

            var ifNoneMatch = context.Request.Headers[HeaderNames.IfNoneMatch].ToString();
            if (!string.IsNullOrEmpty(ifNoneMatch)
                && ifNoneMatch.Split(',', StringSplitOptions.TrimEntries)
                    .Any(tag => tag == etag))
            {
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                context.Response.ContentLength = null;
                context.Response.Body = originalBody;
                return;
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static bool ShouldHandle(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            return false;
        }

        var path = context.Request.Path.Value;
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        return CacheablePathPrefixes.Any(prefix =>
            path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static string ComputeETag(MemoryStream buffer)
    {
        buffer.Position = 0;
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(buffer.GetBuffer().AsSpan(0, (int)buffer.Length), hash);
        return $"\"{Convert.ToHexString(hash[..8])}\"";
    }
}
