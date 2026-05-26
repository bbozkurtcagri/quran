using FluentAssertions;
using QuranCompanion.Application.Common.Errors;
using QuranCompanion.Application.Features.Surahs.Queries.GetSurahDetail;
using QuranCompanion.Application.Tests.Common;

namespace QuranCompanion.Application.Tests.Features.Surahs;

public class GetSurahDetailQueryHandlerTests
{
    [Fact]
    public async Task Returns_detail_for_existing_surah()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetSurahDetailQueryHandler(db);

        var result = await handler.Handle(new GetSurahDetailQuery(2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be(2);
        result.Value.NameTurkish.Should().Be("Bakara");
        result.Value.VerseCount.Should().Be(3);
        result.Value.RevelationPlace.Should().Be("Medinan");
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_surah()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new GetSurahDetailQueryHandler(db);

        var result = await handler.Handle(new GetSurahDetailQuery(50), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("surah.not_found");
    }
}

public class GetSurahDetailQueryValidatorTests
{
    private readonly GetSurahDetailQueryValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(57)]
    [InlineData(114)]
    public void Accepts_valid_surah_numbers(int number)
    {
        var result = _validator.Validate(new GetSurahDetailQuery(number));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(115)]
    [InlineData(-1)]
    public void Rejects_out_of_range_surah_numbers(int number)
    {
        var result = _validator.Validate(new GetSurahDetailQuery(number));
        result.IsValid.Should().BeFalse();
    }
}
