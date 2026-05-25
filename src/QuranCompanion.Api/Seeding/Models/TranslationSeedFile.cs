namespace QuranCompanion.Api.Seeding.Models;

public sealed class TranslationSeedFile
{
    public required string SourceCode { get; init; }

    public required IReadOnlyList<TranslationSeedRecord> Translations { get; init; }
}

public sealed class TranslationSeedRecord
{
    public required int SurahNumber { get; init; }

    public required int VerseNumber { get; init; }

    public required string Text { get; init; }
}
