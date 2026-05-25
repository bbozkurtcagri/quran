namespace QuranCompanion.Application.Features.Verses.Dtos;

public sealed record TranslationDto(
    string SourceCode,
    string SourceName,
    string LanguageCode,
    string Author,
    string Text);
