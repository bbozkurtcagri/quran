namespace QuranCompanion.Application.Features.Verses.Dtos;

public sealed record VerseSummaryDto(
    int SurahNumber,
    int VerseNumber,
    int GlobalVerseNumber,
    int JuzNumber,
    int PageNumber,
    string ArabicText,
    IReadOnlyList<TranslationDto> Translations);
