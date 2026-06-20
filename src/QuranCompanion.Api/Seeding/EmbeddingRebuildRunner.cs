using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using QuranCompanion.Application.Abstractions.Embedding;
using QuranCompanion.Domain.Import;
using QuranCompanion.Domain.Search;
using QuranCompanion.Infrastructure.Persistence;

namespace QuranCompanion.Api.Seeding;

/// <summary>
/// CLI runner that indexes every active Turkish meal under the configured
/// embedder model. Idempotent: rows whose content hash already matches are
/// skipped, so partial runs and re-runs are cheap.
/// </summary>
public sealed class EmbeddingRebuildRunner(
    ApplicationDbContext db,
    IEmbedder embedder,
    ILogger<EmbeddingRebuildRunner> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying migrations…");
        await db.Database.MigrateAsync(cancellationToken);

        var modelName = embedder.ModelName;
        var batchSize = 32;

        var sources = await db.TranslationSources
            .AsNoTracking()
            .Where(s => s.IsActive && s.LanguageCode == "tr")
            .ToListAsync(cancellationToken);

        if (sources.Count == 0)
        {
            logger.LogWarning("No active Turkish translation sources found; nothing to embed.");
            return;
        }

        var totalInserted = 0;
        var totalUpdated = 0;
        var totalSkipped = 0;

        foreach (var source in sources)
        {
            logger.LogInformation(
                "Embedding source {SourceCode} ({SourceName}) under model {Model}…",
                source.Code, source.Name, modelName);

            var translations = await db.VerseTranslations
                .Where(t => t.TranslationSourceId == source.Id)
                .OrderBy(t => t.Verse.GlobalVerseNumber)
                .Select(t => new
                {
                    t.VerseId,
                    t.Text,
                    t.Verse.SurahNumber,
                    t.Verse.VerseNumber,
                })
                .ToListAsync(cancellationToken);

            if (translations.Count == 0)
            {
                logger.LogInformation("  no translations for {SourceCode}, skipping.", source.Code);
                continue;
            }

            // Existing embeddings under this (source, model) — used to short-circuit
            // identical content and to update rows in-place.
            var existing = await db.VerseEmbeddings
                .Where(e => e.TranslationSourceId == source.Id && e.ModelName == modelName)
                .Select(e => new { e.Id, e.VerseId, e.ContentHash })
                .ToDictionaryAsync(e => e.VerseId, cancellationToken);

            var historyId = await StartHistoryAsync(source.Code, modelName, translations.Count, cancellationToken);

            var sourceInserted = 0;
            var sourceUpdated = 0;
            var sourceSkipped = 0;

            for (var offset = 0; offset < translations.Count; offset += batchSize)
            {
                var slice = translations.Skip(offset).Take(batchSize).ToList();

                // First pass: figure out which texts actually need embedding.
                var hashes = slice.Select(s => Hash(s.Text)).ToList();
                var toEmbedIndexes = new List<int>(slice.Count);
                for (var i = 0; i < slice.Count; i++)
                {
                    var verseId = slice[i].VerseId;
                    if (existing.TryGetValue(verseId, out var prev) && prev.ContentHash == hashes[i])
                    {
                        sourceSkipped++;
                        continue;
                    }
                    toEmbedIndexes.Add(i);
                }

                if (toEmbedIndexes.Count == 0)
                {
                    continue;
                }

                var textsToEmbed = toEmbedIndexes.Select(i => slice[i].Text).ToList();
                var vectors = await embedder.EmbedAsync(textsToEmbed, EmbedderKind.Passage, cancellationToken);

                for (var k = 0; k < toEmbedIndexes.Count; k++)
                {
                    var idx = toEmbedIndexes[k];
                    var record = slice[idx];
                    var hash = hashes[idx];
                    var vector = new Vector(vectors[k]);

                    if (existing.TryGetValue(record.VerseId, out var prev))
                    {
                        var tracked = await db.VerseEmbeddings.FindAsync(new object[] { prev.Id }, cancellationToken);
                        if (tracked is not null)
                        {
                            tracked.Embedding = vector;
                            tracked.ContentHash = hash;
                            sourceUpdated++;
                        }
                    }
                    else
                    {
                        db.VerseEmbeddings.Add(new VerseEmbedding
                        {
                            VerseId = record.VerseId,
                            TranslationSourceId = source.Id,
                            ModelName = modelName,
                            ContentHash = hash,
                            Embedding = vector,
                        });
                        sourceInserted++;
                    }
                }

                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation(
                    "  {SourceCode}: {Done}/{Total} processed (inserted={Inserted}, updated={Updated}, skipped={Skipped})",
                    source.Code,
                    Math.Min(offset + batchSize, translations.Count),
                    translations.Count,
                    sourceInserted, sourceUpdated, sourceSkipped);
            }

            await CompleteHistoryAsync(historyId, sourceInserted, sourceUpdated, sourceSkipped, cancellationToken);

            totalInserted += sourceInserted;
            totalUpdated += sourceUpdated;
            totalSkipped += sourceSkipped;
        }

        logger.LogInformation(
            "Embedding rebuild complete. Inserted={Inserted}, Updated={Updated}, Skipped={Skipped}.",
            totalInserted, totalUpdated, totalSkipped);
    }

    private async Task<long> StartHistoryAsync(
        string sourceCode,
        string modelName,
        int verseCount,
        CancellationToken cancellationToken)
    {
        var history = new ImportHistory
        {
            SourceCode = sourceCode,
            ImportType = "embeddings",
            FileName = modelName,
            ContentHash = $"verses={verseCount}",
            Status = ImportStatus.Running,
            StartedOnUtc = DateTime.UtcNow,
        };
        db.ImportHistories.Add(history);
        await db.SaveChangesAsync(cancellationToken);
        return history.Id;
    }

    private async Task CompleteHistoryAsync(
        long historyId,
        int inserted,
        int updated,
        int skipped,
        CancellationToken cancellationToken)
    {
        var history = await db.ImportHistories.FindAsync(new object[] { historyId }, cancellationToken);
        if (history is null)
        {
            return;
        }
        history.Status = ImportStatus.Succeeded;
        history.InsertedCount = inserted;
        history.UpdatedCount = updated;
        history.FailedCount = 0;
        history.CompletedOnUtc = DateTime.UtcNow;
        // record skipped via error message slot — semantically misuse but cheap
        history.ErrorMessage = skipped > 0 ? $"skipped (already up-to-date): {skipped}" : null;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string Hash(string text)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(text ?? string.Empty), hash);
        return Convert.ToHexString(hash[..16]); // 32 hex chars
    }
}
