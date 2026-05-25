namespace QuranCompanion.Application.Features.TranslationSources.Dtos;

public sealed record TranslationSourceDto(
    string Code,
    string Name,
    string LanguageCode,
    string Author,
    string? Description,
    string? LicenseInfo,
    string? SourceUrl,
    bool IsDefault);
