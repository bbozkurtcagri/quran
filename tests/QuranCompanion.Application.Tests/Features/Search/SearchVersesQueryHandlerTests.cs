using FluentAssertions;
using NSubstitute;
using QuranCompanion.Application.Abstractions.Embedding;
using QuranCompanion.Application.Common.Text;
using QuranCompanion.Application.Features.Search.Queries.SearchVerses;
using QuranCompanion.Application.Tests.Common;

namespace QuranCompanion.Application.Tests.Features.Search;

public class SearchVersesQueryHandlerTests
{
    [Fact]
    public async Task Returns_empty_for_empty_normalized_query()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new SearchVersesQueryHandler(db, new DefaultTextNormalizer(), Substitute.For<IEmbedder>());

        var result = await handler.Handle(
            new SearchVersesQuery("   ", null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Finds_verses_matching_normalized_query()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new SearchVersesQueryHandler(db, new DefaultTextNormalizer(), Substitute.For<IEmbedder>());

        var result = await handler.Handle(
            new SearchVersesQuery("sabır", null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2,
            because: "two seeded verses mention sabredenler/sabırlı in their meals");
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().OnlyContain(h => h.SurahNumber == 2);
    }

    [Fact]
    public async Task Match_is_case_and_diacritic_insensitive()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new SearchVersesQueryHandler(db, new DefaultTextNormalizer(), Substitute.For<IEmbedder>());

        var upper = await handler.Handle(
            new SearchVersesQuery("SABIR", null, 1, 20),
            CancellationToken.None);

        upper.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Honors_source_filter()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new SearchVersesQueryHandler(db, new DefaultTextNormalizer(), Substitute.For<IEmbedder>());

        var hit = await handler.Handle(
            new SearchVersesQuery("sabır", "elmalili", 1, 20),
            CancellationToken.None);
        hit.Value.TotalCount.Should().Be(2);

        var miss = await handler.Handle(
            new SearchVersesQuery("sabır", "unknown-source", 1, 20),
            CancellationToken.None);
        miss.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Pages_results()
    {
        await using var db = TestDb.Create();
        await SampleData.SeedAsync(db);

        var handler = new SearchVersesQueryHandler(db, new DefaultTextNormalizer(), Substitute.For<IEmbedder>());

        var page1 = await handler.Handle(
            new SearchVersesQuery("sabır", null, 1, 1),
            CancellationToken.None);
        var page2 = await handler.Handle(
            new SearchVersesQuery("sabır", null, 2, 1),
            CancellationToken.None);

        page1.Value.TotalCount.Should().Be(2);
        page1.Value.Items.Should().HaveCount(1);
        page2.Value.Items.Should().HaveCount(1);
        page1.Value.Items[0].VerseNumber.Should().NotBe(page2.Value.Items[0].VerseNumber);
    }
}

public class SearchVersesQueryValidatorTests
{
    private readonly SearchVersesQueryValidator _validator = new();

    [Fact]
    public void Rejects_empty_query()
    {
        var result = _validator.Validate(new SearchVersesQuery("", null, 1, 20));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Rejects_query_below_minimum_length()
    {
        var result = _validator.Validate(new SearchVersesQuery("a", null, 1, 20));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Rejects_page_size_above_max()
    {
        var result = _validator.Validate(new SearchVersesQuery("sabır", null, 1, 1000));
        result.IsValid.Should().BeFalse();
    }
}
