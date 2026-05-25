namespace QuranCompanion.Api.Seeding.Models;

public sealed class VerseSeedRecord
{
    public required int SurahNumber { get; init; }

    public required int VerseNumber { get; init; }

    public required int GlobalVerseNumber { get; init; }

    public required int JuzNumber { get; init; }

    public required int PageNumber { get; init; }

    public required string ArabicText { get; init; }
}
