namespace QuranCompanion.Application.Features.Surahs.Dtos;

public sealed record SurahListItemDto(
    int Number,
    string NameArabic,
    string NameTurkish,
    string NameTransliteration,
    int VerseCount,
    string RevelationPlace);
