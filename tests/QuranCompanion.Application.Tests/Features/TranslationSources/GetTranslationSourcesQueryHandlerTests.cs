using FluentAssertions;
using QuranCompanion.Application.Features.TranslationSources.Queries.GetTranslationSources;
using QuranCompanion.Application.Tests.Common;

namespace QuranCompanion.Application.Tests.Features.TranslationSources;

public class GetTranslationSourcesQueryHandlerTests
{
    [Fact]
    public async Task Returns_only_active_sources()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetTranslationSourcesQueryHandler(db);

        var result = await handler.Handle(
            new GetTranslationSourcesQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(s => s.Code).Should().NotContain("inactive-test");
    }

    [Fact]
    public async Task Default_source_comes_first()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetTranslationSourcesQueryHandler(db);

        var result = await handler.Handle(
            new GetTranslationSourcesQuery(),
            CancellationToken.None);

        result.Value[0].IsDefault.Should().BeTrue();
        result.Value[0].Code.Should().Be("elmalili");
    }

    [Fact]
    public async Task Filters_by_language_code()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetTranslationSourcesQueryHandler(db);

        var result = await handler.Handle(
            new GetTranslationSourcesQuery("ar"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Code.Should().Be("tanzil-uthmani-minimal");
    }
}
