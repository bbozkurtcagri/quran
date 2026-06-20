using Microsoft.EntityFrameworkCore;
using QuranCompanion.Domain.Import;
using QuranCompanion.Domain.Quran;
using QuranCompanion.Domain.Search;

namespace QuranCompanion.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Surah> Surahs { get; }

    DbSet<Verse> Verses { get; }

    DbSet<TranslationSource> TranslationSources { get; }

    DbSet<VerseTranslation> VerseTranslations { get; }

    DbSet<ImportHistory> ImportHistories { get; }

    DbSet<VerseEmbedding> VerseEmbeddings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
