using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace QuranCompanion.Api.Common;

/// <summary>
/// Sets a <c>Cache-Control</c> header on the response after the action runs.
/// Use a long max-age for endpoints serving immutable Quran data; use
/// <c>no-store</c> for search and health.
///
/// In Development, any policy that contains <c>immutable</c> is rewritten to
/// <c>no-cache</c> so devs always revalidate against the server (preventing
/// confusing situations where a browser disk-cache hit from a previous
/// schema version shadows the current API response). The ETag pipeline
/// continues to short-circuit unchanged content to 304.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class CacheControlAttribute(string value) : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var effective = value;

        var env = context.HttpContext.RequestServices
            .GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
        if (env is not null && env.IsDevelopment()
            && value.Contains("immutable", StringComparison.OrdinalIgnoreCase))
        {
            effective = "no-cache";
        }

        context.HttpContext.Response.Headers[HeaderNames.CacheControl] = effective;
        base.OnResultExecuting(context);
    }
}

public static class CachePolicies
{
    /// <summary>
    /// Immutable Quran data (surahs, verses, single-verse meals): cache for
    /// a year client-side. The seed CLI is the only mechanism that changes
    /// this data and that happens out-of-band; clients can revalidate via
    /// the ETag (304) when they think there might be a new release.
    /// </summary>
    public const string Immutable = "public, max-age=31536000, immutable";

    /// <summary>
    /// Translation sources list: changes rarely (when admins add or
    /// disable a source). One-hour client cache + ETag revalidation.
    /// </summary>
    public const string TranslationSources = "public, max-age=3600";

    /// <summary>Search and health endpoints must not be cached.</summary>
    public const string NoStore = "no-store";
}
