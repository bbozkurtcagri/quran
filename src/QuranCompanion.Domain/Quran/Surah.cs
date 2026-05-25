using QuranCompanion.Domain.Common;

namespace QuranCompanion.Domain.Quran;

public class Surah : BaseEntity
{
    public int Number { get; set; }

    public string NameArabic { get; set; } = null!;

    public string NameTurkish { get; set; } = null!;

    public string NameTransliteration { get; set; } = null!;

    public int VerseCount { get; set; }

    public RevelationPlace RevelationPlace { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<Verse> Verses { get; set; } = new List<Verse>();
}
