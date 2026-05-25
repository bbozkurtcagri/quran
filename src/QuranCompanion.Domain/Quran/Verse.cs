using QuranCompanion.Domain.Common;

namespace QuranCompanion.Domain.Quran;

public class Verse : BaseEntity
{
    public long SurahId { get; set; }

    public Surah Surah { get; set; } = null!;

    public int SurahNumber { get; set; }

    public int VerseNumber { get; set; }

    public int GlobalVerseNumber { get; set; }

    public int JuzNumber { get; set; }

    public int PageNumber { get; set; }

    public string ArabicText { get; set; } = null!;

    public string ArabicTextClean { get; set; } = null!;

    public ICollection<VerseTranslation> Translations { get; set; } = new List<VerseTranslation>();
}
