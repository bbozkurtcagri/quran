using System.Globalization;
using System.Text;
using QuranCompanion.Application.Abstractions.Text;

namespace QuranCompanion.Application.Common.Text;

internal sealed class DefaultTextNormalizer : ITextNormalizer
{
    private static readonly CultureInfo TurkishCulture = new("tr-TR");

    // NFKD does not decompose Turkish dotless ı; map it (and dotted İ) to i
    // explicitly so search matches across both spellings.
    private static readonly Dictionary<char, char> TurkishMap = new()
    {
        ['ı'] = 'i',
        ['İ'] = 'i',
    };

    public string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var lowered = text.ToLower(TurkishCulture);

        var mapped = new StringBuilder(lowered.Length);
        foreach (var ch in lowered)
        {
            mapped.Append(TurkishMap.TryGetValue(ch, out var replacement) ? replacement : ch);
        }

        var decomposed = mapped.ToString().Normalize(NormalizationForm.FormKD);

        var builder = new StringBuilder(decomposed.Length);
        var prevWasSpace = false;

        foreach (var ch in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(ch))
            {
                if (!prevWasSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                    prevWasSpace = true;
                }
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                prevWasSpace = false;
            }
            else
            {
                // Drop punctuation entirely; treat as a soft separator.
                if (!prevWasSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                    prevWasSpace = true;
                }
            }
        }

        return builder.ToString().TrimEnd();
    }
}
