using Microsoft.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Domain.Common;
using QuranCompanion.Domain.Import;
using QuranCompanion.Domain.Quran;
using QuranCompanion.Domain.Search;

namespace QuranCompanion.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IClock clock)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Surah> Surahs => Set<Surah>();

    public DbSet<Verse> Verses => Set<Verse>();

    public DbSet<TranslationSource> TranslationSources => Set<TranslationSource>();

    public DbSet<VerseTranslation> VerseTranslations => Set<VerseTranslation>();

    public DbSet<ImportHistory> ImportHistories => Set<ImportHistory>();

    public DbSet<VerseEmbedding> VerseEmbeddings => Set<VerseEmbedding>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = clock.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOnUtc = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedOnUtc = utcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.IsRelational())
        {
            modelBuilder.HasPostgresExtension("pg_trgm");
            modelBuilder.HasPostgresExtension("unaccent");
            modelBuilder.HasPostgresExtension("vector");
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // pgvector's Vector type has no in-memory mapping; integration tests
        // use the EF in-memory provider, so we drop the entity from the model
        // when the configured provider is not relational.
        if (!Database.IsRelational())
        {
            modelBuilder.Ignore<VerseEmbedding>();
        }

        base.OnModelCreating(modelBuilder);
    }
}
