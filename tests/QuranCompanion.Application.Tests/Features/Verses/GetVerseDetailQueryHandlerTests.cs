using FluentAssertions;
using QuranCompanion.Application.Common.Errors;
using QuranCompanion.Application.Features.Verses.Queries.GetVerseDetail;
using QuranCompanion.Application.Tests.Common;

namespace QuranCompanion.Application.Tests.Features.Verses;

public class GetVerseDetailQueryHandlerTests
{
    [Fact]
    public async Task Returns_verse_with_default_translations()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetVerseDetailQueryHandler(db);

        var result = await handler.Handle(
            new GetVerseDetailQuery(112, 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SurahNumber.Should().Be(112);
        result.Value.VerseNumber.Should().Be(1);
        result.Value.ArabicText.Should().Contain("قُل");
        result.Value.Translations.Should().HaveCount(1);
        result.Value.Translations[0].Text.Should().Contain("bir tektir");
    }

    [Fact]
    public async Task Returns_not_found_for_missing_verse()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetVerseDetailQueryHandler(db);

        var result = await handler.Handle(
            new GetVerseDetailQuery(2, 99, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("verse.not_found");
    }

    [Fact]
    public async Task Filters_translations_by_source()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetVerseDetailQueryHandler(db);

        var result = await handler.Handle(
            new GetVerseDetailQuery(1, 1, "tanzil-uthmani-minimal"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Translations.Should().BeEmpty(
            because: "the sample data has no Arabic-source verse translations");
    }
}
