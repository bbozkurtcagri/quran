using QuranCompanion.Domain.Common;

namespace QuranCompanion.Domain.Quran;

public class TranslationSource : BaseEntity
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string LanguageCode { get; set; } = "tr";

    public string Author { get; set; } = null!;

    public string? Description { get; set; }

    public string? LicenseInfo { get; set; }

    public string? SourceUrl { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public ICollection<VerseTranslation> Translations { get; set; } = new List<VerseTranslation>();
}
