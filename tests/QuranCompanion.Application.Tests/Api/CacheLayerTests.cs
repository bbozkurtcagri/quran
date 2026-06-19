using System.Net;
using FluentAssertions;

namespace QuranCompanion.Application.Tests.Api;

public class CacheLayerTests : IAsyncLifetime
{
    private CachingApiFactory _factory = null!;

    public async Task InitializeAsync()
    {
        _factory = new CachingApiFactory();
        await _factory.SeedAsync();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Surah_list_returns_full_body_with_etag_and_cache_control()
    {
        using var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/v1/surahs");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Headers.ETag.Should().NotBeNull();
        res.Headers.ETag!.Tag.Should().StartWith("\"").And.EndWith("\"");
        res.Headers.CacheControl.Should().NotBeNull();
        res.Headers.CacheControl!.ToString().Should().Contain("max-age");

        var json = await res.Content.ReadAsStringAsync();
        json.Should().Contain("\"data\":");
        json.Should().Contain("Fâtiha");
    }

    [Fact]
    public async Task Cached_response_body_matches_first_response_byte_for_byte()
    {
        using var client = _factory.CreateClient();

        var first = await client.GetAsync("/api/v1/surahs");
        var firstBody = await first.Content.ReadAsByteArrayAsync();
        var firstEtag = first.Headers.ETag!.Tag;

        var second = await client.GetAsync("/api/v1/surahs");
        var secondBody = await second.Content.ReadAsByteArrayAsync();
        var secondEtag = second.Headers.ETag!.Tag;

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        secondEtag.Should().Be(firstEtag);
        secondBody.Should().Equal(firstBody);
    }

    [Fact]
    public async Task If_none_match_returns_304_with_empty_body()
    {
        using var client = _factory.CreateClient();

        var first = await client.GetAsync("/api/v1/surahs");
        var etag = first.Headers.ETag!.Tag;

        using var conditional = new HttpRequestMessage(HttpMethod.Get, "/api/v1/surahs");
        conditional.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var res = await client.SendAsync(conditional);

        res.StatusCode.Should().Be(HttpStatusCode.NotModified);
        var body = await res.Content.ReadAsByteArrayAsync();
        body.Length.Should().Be(0);
    }

    [Fact]
    public async Task Concurrent_requests_each_get_a_complete_body()
    {
        // Regression guard for the StrictMode double-effect bug: with
        // OutputCache locking on, a concurrent waiter could read an empty
        // body when the leader aborted. With locking off (and the ETag
        // middleware using IHttpResponseBodyFeature), every concurrent
        // request must receive an independent, fully-formed body.
        using var client = _factory.CreateClient();

        var tasks = Enumerable.Range(0, 8)
            .Select(_ => client.GetAsync("/api/v1/surahs"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        foreach (var res in responses)
        {
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await res.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrEmpty();
            body.Should().Contain("\"success\":true");
            body.Should().Contain("Fâtiha");
        }
    }

    [Fact]
    public async Task Search_response_is_not_cached_and_has_no_etag()
    {
        using var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/v1/search?query=sabir");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Headers.CacheControl?.NoStore.Should().BeTrue();
        // ETag middleware only handles surahs/verses/translation-sources paths.
        res.Headers.ETag.Should().BeNull();
    }
}
