namespace QuranCompanion.Api.Seeding.Models;

public sealed class SurahSeedRecord
{
    public required int Number { get; init; }

    public required string NameArabic { get; init; }

    public required string NameTurkish { get; init; }

    public required string NameTransliteration { get; init; }

    public required int VerseCount { get; init; }

    public required string RevelationPlace { get; init; }

    public int? DisplayOrder { get; init; }
}
