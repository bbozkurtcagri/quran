using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using QuranCompanion.Api.Common;

namespace QuranCompanion.Application.Tests.Api;

public class ETagMiddlewareTests
{
    private static async Task<DefaultHttpContext> RunAsync(
        string path,
        string method,
        Func<HttpContext, Task> action,
        string? ifNoneMatch = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        if (ifNoneMatch is not null)
        {
            context.Request.Headers["If-None-Match"] = ifNoneMatch;
        }
        context.Response.Body = new MemoryStream();

        RequestDelegate next = ctx => action(ctx);
        var middleware = new ETagMiddleware(next);
        await middleware.InvokeAsync(context);

        return context;
    }

    [Fact]
    public async Task Adds_etag_header_to_cacheable_get_response()
    {
        var ctx = await RunAsync(
            "/api/v1/surahs",
            HttpMethods.Get,
            async http =>
            {
                http.Response.StatusCode = 200;
                http.Response.ContentType = "application/json";
                await http.Response.WriteAsync("{\"data\":[]}");
            });

        ctx.Response.Headers.ETag.ToString().Should().StartWith("\"").And.EndWith("\"");
    }

    [Fact]
    public async Task Returns_304_when_if_none_match_matches()
    {
        // First pass — get the ETag.
        var first = await RunAsync(
            "/api/v1/surahs",
            HttpMethods.Get,
            async http =>
            {
                http.Response.StatusCode = 200;
                await http.Response.WriteAsync("{\"data\":[]}");
            });

        var etag = first.Response.Headers.ETag.ToString();
        etag.Should().NotBeEmpty();

        // Second pass with matching If-None-Match.
        var second = await RunAsync(
            "/api/v1/surahs",
            HttpMethods.Get,
            async http =>
            {
                http.Response.StatusCode = 200;
                await http.Response.WriteAsync("{\"data\":[]}");
            },
            ifNoneMatch: etag);

        second.Response.StatusCode.Should().Be(304);
    }

    [Fact]
    public async Task Skips_non_cacheable_paths()
    {
        var ctx = await RunAsync(
            "/api/v1/search?query=sabir",
            HttpMethods.Get,
            async http =>
            {
                http.Response.StatusCode = 200;
                await http.Response.WriteAsync("{\"data\":[]}");
            });

        ctx.Response.Headers.ETag.ToString().Should().BeEmpty();
    }

    [Fact]
    public async Task Skips_non_get_methods()
    {
        var ctx = await RunAsync(
            "/api/v1/surahs",
            HttpMethods.Post,
            async http =>
            {
                http.Response.StatusCode = 200;
                await http.Response.WriteAsync("{}");
            });

        ctx.Response.Headers.ETag.ToString().Should().BeEmpty();
    }

    [Fact]
    public async Task Skips_non_200_responses()
    {
        var ctx = await RunAsync(
            "/api/v1/surahs/999",
            HttpMethods.Get,
            async http =>
            {
                http.Response.StatusCode = 404;
                await http.Response.WriteAsync("{\"error\":\"not found\"}");
            });

        ctx.Response.Headers.ETag.ToString().Should().BeEmpty();
    }
}
