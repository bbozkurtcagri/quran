using FluentAssertions;
using QuranCompanion.Application.Abstractions.Text;
using QuranCompanion.Application.Common.Text;

namespace QuranCompanion.Application.Tests.Common.Text;

public class DefaultTextNormalizerTests
{
    private readonly ITextNormalizer _normalizer = new DefaultTextNormalizer();

    [Fact]
    public void Returns_empty_for_null_or_whitespace()
    {
        _normalizer.Normalize(null).Should().BeEmpty();
        _normalizer.Normalize("").Should().BeEmpty();
        _normalizer.Normalize("   ").Should().BeEmpty();
    }

    [Theory]
    [InlineData("Sabır", "sabir")]
    [InlineData("ŞÜKÜR", "sukur")]
    [InlineData("İSTİĞFAR", "istigfar")]
    [InlineData("namaz", "namaz")]
    public void Lowercases_and_strips_turkish_diacritics(string input, string expected)
    {
        _normalizer.Normalize(input).Should().Be(expected);
    }

    [Fact]
    public void Collapses_whitespace_and_drops_punctuation()
    {
        _normalizer.Normalize("  Hello,   world!  ").Should().Be("hello world");
    }

    [Fact]
    public void Trims_trailing_separator()
    {
        _normalizer.Normalize("test.").Should().Be("test");
    }
}
