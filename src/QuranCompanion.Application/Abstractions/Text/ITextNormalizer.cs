namespace QuranCompanion.Application.Abstractions.Text;

public interface ITextNormalizer
{
    /// <summary>
    /// Produces a lower-cased, diacritic-stripped, whitespace-collapsed form of
    /// the input suitable for case- and accent-insensitive search.
    /// </summary>
    string Normalize(string? text);
}
