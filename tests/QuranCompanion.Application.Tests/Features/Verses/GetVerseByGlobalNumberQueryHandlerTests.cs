using FluentAssertions;
using QuranCompanion.Application.Common.Errors;
using QuranCompanion.Application.Features.Verses.Queries.GetVerseByGlobalNumber;
using QuranCompanion.Application.Tests.Common;

namespace QuranCompanion.Application.Tests.Features.Verses;

public class GetVerseByGlobalNumberQueryHandlerTests
{
    [Fact]
    public async Task Returns_verse_by_global_number()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetVerseByGlobalNumberQueryHandler(db);

        var result = await handler.Handle(
            new GetVerseByGlobalNumberQuery(7, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SurahNumber.Should().Be(112);
        result.Value.VerseNumber.Should().Be(1);
        result.Value.GlobalVerseNumber.Should().Be(7);
    }

    [Fact]
    public async Task Returns_not_found_for_missing_global_number()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetVerseByGlobalNumberQueryHandler(db);

        var result = await handler.Handle(
            new GetVerseByGlobalNumberQuery(999, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
