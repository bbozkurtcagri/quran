using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

namespace QuranCompanion.Api.Common;

/// <summary>
/// Adds an <c>ETag</c> header to successful GET responses and short-circuits
/// to 304 Not Modified when the client's <c>If-None-Match</c> matches.
///
/// Uses the modern <see cref="IHttpResponseBodyFeature"/> pattern: we wrap the
/// existing feature with one whose stream is a <see cref="MemoryStream"/>, so
/// that any middleware that also wraps the response body (e.g. OutputCache)
/// keeps working. After the inner pipeline runs we read the buffer, compute a
/// SHA-256 ETag (first 16 hex chars), set the header, and either copy the
/// buffer through to the original body or return 304 with no body.
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

        var originalFeature = context.Features.Get<IHttpResponseBodyFeature>()
            ?? throw new InvalidOperationException("Response body feature missing.");

        using var buffer = new MemoryStream();
        var wrappedFeature = new StreamResponseBodyFeature(buffer);
        context.Features.Set<IHttpResponseBodyFeature>(wrappedFeature);

        try
        {
            await next(context);
            await wrappedFeature.CompleteAsync();
        }
        finally
        {
            context.Features.Set(originalFeature);
        }

        if (context.Response.StatusCode != StatusCodes.Status200OK || buffer.Length == 0)
        {
            buffer.Position = 0;
            await buffer.CopyToAsync(originalFeature.Stream);
            return;
        }

        var etag = ComputeETag(buffer);
        context.Response.Headers[HeaderNames.ETag] = etag;

        var ifNoneMatch = context.Request.Headers[HeaderNames.IfNoneMatch].ToString();
        if (!string.IsNullOrEmpty(ifNoneMatch)
            && ifNoneMatch.Split(',', StringSplitOptions.TrimEntries).Any(tag => tag == etag))
        {
            context.Response.StatusCode = StatusCodes.Status304NotModified;
            context.Response.ContentLength = null;
            // No body for 304; we deliberately don't copy the buffer.
            return;
        }

        buffer.Position = 0;
        // Make sure Content-Length matches the actual buffer, regardless of
        // what the inner pipeline thought it was writing.
        context.Response.ContentLength = buffer.Length;
        await buffer.CopyToAsync(originalFeature.Stream);
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
