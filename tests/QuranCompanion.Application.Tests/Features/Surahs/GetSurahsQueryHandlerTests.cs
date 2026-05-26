using FluentAssertions;
using QuranCompanion.Application.Features.Surahs.Queries.GetSurahs;
using QuranCompanion.Application.Tests.Common;

namespace QuranCompanion.Application.Tests.Features.Surahs;

public class GetSurahsQueryHandlerTests
{
    [Fact]
    public async Task Returns_empty_when_no_surahs_exist()
    {
        await using var db = TestDb.Create();
        var handler = new GetSurahsQueryHandler(db);

        var result = await handler.Handle(new GetSurahsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_all_surahs_ordered_by_display_order()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetSurahsQueryHandler(db);

        var result = await handler.Handle(new GetSurahsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Select(s => s.Number).Should().Equal(1, 2, 112);
    }

    [Fact]
    public async Task Projects_revelation_place_as_string()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetSurahsQueryHandler(db);

        var result = await handler.Handle(new GetSurahsQuery(), CancellationToken.None);

        result.Value.Single(s => s.Number == 1).RevelationPlace.Should().Be("Meccan");
        result.Value.Single(s => s.Number == 2).RevelationPlace.Should().Be("Medinan");
    }
}
