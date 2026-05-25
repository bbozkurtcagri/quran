using Microsoft.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Domain.Common;
using QuranCompanion.Domain.Import;
using QuranCompanion.Domain.Quran;

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
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("unaccent");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
