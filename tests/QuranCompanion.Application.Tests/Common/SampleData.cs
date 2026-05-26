using QuranCompanion.Application.Abstractions.Text;
using QuranCompanion.Application.Common.Text;
using QuranCompanion.Domain.Quran;
using QuranCompanion.Infrastructure.Persistence;

namespace QuranCompanion.Application.Tests.Common;

/// <summary>
/// Minimal but realistic fixture used across handler tests. Three surahs,
/// nine verses, two translation sources (Arabic + Turkish, the Turkish one
/// being the default), and a Turkish meal for every verse.
/// </summary>
internal static class SampleData
{
    private static readonly ITextNormalizer Normalizer = new DefaultTextNormalizer();

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var arabicSource = new TranslationSource
        {
            Code = "tanzil-uthmani-minimal",
            Name = "Tanzil — Uthmani Minimal",
            LanguageCode = "ar",
            Author = "Tanzil Project",
            IsDefault = false,
            IsActive = true,
        };

        var elmalili = new TranslationSource
        {
            Code = "elmalili",
            Name = "Elmalılı Hamdi Yazır — Meal",
            LanguageCode = "tr",
            Author = "Elmalılı Muhammed Hamdi Yazır",
            IsDefault = true,
            IsActive = true,
        };

        // An inactive Turkish source so we can prove the active filter works.
        var inactiveTurkish = new TranslationSource
        {
            Code = "inactive-test",
            Name = "Inactive Test",
            LanguageCode = "tr",
            Author = "Test",
            IsDefault = false,
            IsActive = false,
        };

        db.TranslationSources.AddRange(arabicSource, elmalili, inactiveTurkish);

        var surahs = new[]
        {
            new Surah
            {
                Number = 1,
                NameArabic = "الفاتحة",
                NameTurkish = "Fâtiha",
                NameTransliteration = "Al-Fatihah",
                VerseCount = 3,
                RevelationPlace = RevelationPlace.Meccan,
                DisplayOrder = 1,
            },
            new Surah
            {
                Number = 2,
                NameArabic = "البقرة",
                NameTurkish = "Bakara",
                NameTransliteration = "Al-Baqarah",
                VerseCount = 3,
                RevelationPlace = RevelationPlace.Medinan,
                DisplayOrder = 2,
            },
            new Surah
            {
                Number = 112,
                NameArabic = "الإخلاص",
                NameTurkish = "İhlas",
                NameTransliteration = "Al-Ikhlas",
                VerseCount = 3,
                RevelationPlace = RevelationPlace.Meccan,
                DisplayOrder = 112,
            },
        };
        db.Surahs.AddRange(surahs);

        await db.SaveChangesAsync();

        // Verses + Turkish meals. globalVerseNumber is fabricated for the test;
        // real data uses the mushaf numbering.
        var verses = new (int surahNumber, int verseNumber, int global, string ar, string tr)[]
        {
            (1, 1, 1, "بِسمِ اللَّهِ الرَّحمٰنِ الرَّحيمِ", "Rahmân ve Rahîm olan Allah'ın ismiyle."),
            (1, 2, 2, "الحَمدُ لِلَّهِ", "Hamd Allah'a mahsustur."),
            (1, 3, 3, "اهدِنَا الصِّرٰطَ", "Bizi doğru yola ilet."),

            (2, 1, 4, "الم", "Elif, Lâm, Mîm."),
            (2, 2, 5, "ذٰلِكَ الكِتٰبُ", "Sabır ve namaz Allah'tan yardımdır."),
            (2, 3, 6, "الَّذينَ يُؤمِنونَ بِالغَيبِ", "SABIR gösterenlerin yardımcısı Allah'tır."),

            (112, 1, 7, "قُل هُوَ اللَّهُ أَحَدٌ", "De ki; O Allah bir tektir."),
            (112, 2, 8, "اللَّهُ الصَّمَدُ", "Allah eksiksiz ve samed'tir."),
            (112, 3, 9, "لَم يَلِد", "Doğurmadı ve doğurulmadı."),
        };

        foreach (var (surahNumber, verseNumber, global, ar, tr) in verses)
        {
            var surah = surahs.First(s => s.Number == surahNumber);

            var verse = new Verse
            {
                SurahId = surah.Id,
                SurahNumber = surahNumber,
                VerseNumber = verseNumber,
                GlobalVerseNumber = global,
                JuzNumber = 1,
                PageNumber = 1,
                ArabicText = ar,
                ArabicTextClean = Normalizer.Normalize(ar),
            };
            db.Verses.Add(verse);

            await db.SaveChangesAsync();

            db.VerseTranslations.Add(new VerseTranslation
            {
                VerseId = verse.Id,
                TranslationSourceId = elmalili.Id,
                Text = tr,
                NormalizedText = Normalizer.Normalize(tr),
            });
        }

        await db.SaveChangesAsync();
    }
}
