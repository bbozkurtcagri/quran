namespace QuranCompanion.Api.Seeding.Models;

public sealed class TranslationSourceSeedRecord
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required string LanguageCode { get; init; }

    public required string Author { get; init; }

    public string? Description { get; init; }

    public string? LicenseInfo { get; init; }

    public string? SourceUrl { get; init; }

    public bool IsDefault { get; init; }

    public bool IsActive { get; init; } = true;
}
