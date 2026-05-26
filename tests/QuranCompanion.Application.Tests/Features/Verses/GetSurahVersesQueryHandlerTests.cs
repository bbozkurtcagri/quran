using FluentAssertions;
using QuranCompanion.Application.Common.Errors;
using QuranCompanion.Application.Features.Verses.Queries.GetSurahVerses;
using QuranCompanion.Application.Tests.Common;

namespace QuranCompanion.Application.Tests.Features.Verses;

public class GetSurahVersesQueryHandlerTests
{
    [Fact]
    public async Task Returns_all_verses_of_surah_with_translations()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetSurahVersesQueryHandler(db);

        var result = await handler.Handle(
            new GetSurahVersesQuery(2, null, 1, 10),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.Items.Select(v => v.VerseNumber).Should().Equal(1, 2, 3);
        result.Value.Items[0].Translations.Should().HaveCount(1);
        result.Value.Items[0].Translations[0].SourceCode.Should().Be("elmalili");
    }

    [Fact]
    public async Task Filters_translations_by_source_code()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetSurahVersesQueryHandler(db);

        var result = await handler.Handle(
            new GetSurahVersesQuery(1, "nonexistent-source", 1, 10),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.Items.Should().OnlyContain(v => v.Translations.Count == 0);
    }

    [Fact]
    public async Task Pages_results()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetSurahVersesQueryHandler(db);

        var page1 = await handler.Handle(
            new GetSurahVersesQuery(1, null, 1, 2),
            CancellationToken.None);
        var page2 = await handler.Handle(
            new GetSurahVersesQuery(1, null, 2, 2),
            CancellationToken.None);

        page1.Value.Items.Should().HaveCount(2);
        page1.Value.Items.Select(v => v.VerseNumber).Should().Equal(1, 2);
        page1.Value.TotalCount.Should().Be(3);

        page2.Value.Items.Should().HaveCount(1);
        page2.Value.Items.Select(v => v.VerseNumber).Should().Equal(3);
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_surah()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetSurahVersesQueryHandler(db);

        var result = await handler.Handle(
            new GetSurahVersesQuery(50, null, 1, 10),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
