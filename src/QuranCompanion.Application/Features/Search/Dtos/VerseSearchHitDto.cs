namespace QuranCompanion.Application.Features.Search.Dtos;

public sealed record VerseSearchHitDto(
    int SurahNumber,
    string SurahNameTurkish,
    int VerseNumber,
    int GlobalVerseNumber,
    string ArabicText,
    string TranslationSourceCode,
    string TranslationText);
