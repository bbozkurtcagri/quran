using QuranCompanion.Domain.Common;

namespace QuranCompanion.Domain.Quran;

public class VerseTranslation : BaseEntity
{
    public long VerseId { get; set; }

    public Verse Verse { get; set; } = null!;

    public long TranslationSourceId { get; set; }

    public TranslationSource TranslationSource { get; set; } = null!;

    public string Text { get; set; } = null!;

    public string NormalizedText { get; set; } = null!;
}
