using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuranCompanion.Api.Seeding.Models;
using QuranCompanion.Application.Abstractions.Text;
using QuranCompanion.Domain.Import;
using QuranCompanion.Domain.Quran;
using QuranCompanion.Infrastructure.Persistence;

namespace QuranCompanion.Api.Seeding;

public sealed class SeedRunner(
    ApplicationDbContext db,
    ITextNormalizer normalizer,
    ILogger<SeedRunner> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task RunAsync(string seedDirectory, CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying migrations…");
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migrations applied.");

        await ImportFromFileAsync(
            Path.Combine(seedDirectory, "quran", "surahs.json"),
            ImportType.Surahs,
            "quran",
            ImportSurahsAsync,
            cancellationToken);

        await ImportFromFileAsync(
            Path.Combine(seedDirectory, "quran", "verses.json"),
            ImportType.Verses,
            "quran",
            ImportVersesAsync,
            cancellationToken);

        var translationSourcesPath = Path.Combine(seedDirectory, "translations", "sources.json");
        await ImportFromFileAsync(
            translationSourcesPath,
            "translation_sources",
            "translations",
            ImportTranslationSourcesAsync,
            cancellationToken);

        var translationsDir = Path.Combine(seedDirectory, "translations");
        if (Directory.Exists(translationsDir))
        {
            foreach (var file in Directory.EnumerateFiles(translationsDir, "*.json")
                .Where(p => !p.EndsWith("sources.json", StringComparison.OrdinalIgnoreCase)))
            {
                await ImportFromFileAsync(
                    file,
                    ImportType.Translations,
                    Path.GetFileNameWithoutExtension(file),
                    ImportTranslationsAsync,
                    cancellationToken);
            }
        }
    }

    private async Task ImportFromFileAsync(
        string path,
        string importType,
        string sourceCode,
        Func<string, CancellationToken, Task<(int Inserted, int Updated)>> handler,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            logger.LogInformation(
                "Seed file {Path} not found; skipping {ImportType} import.",
                path,
                importType);
            return;
        }

        var fileName = Path.GetFileName(path);
        var content = await File.ReadAllTextAsync(path, cancellationToken);

        if (string.IsNullOrWhiteSpace(content) || content.Trim() is "[]" or "{}")
        {
            logger.LogInformation(
                "Seed file {Path} is empty; skipping {ImportType} import.",
                path,
                importType);
            return;
        }

        var hash = ComputeHash(content);

        var alreadyImported = await db.ImportHistories
            .AnyAsync(
                h => h.SourceCode == sourceCode
                    && h.ImportType == importType
                    && h.ContentHash == hash
                    && h.Status == ImportStatus.Succeeded,
                cancellationToken);

        if (alreadyImported)
        {
            logger.LogInformation(
                "Seed file {Path} already imported (hash {Hash}); skipping.",
                path,
                hash);
            return;
        }

        var history = new ImportHistory
        {
            SourceCode = sourceCode,
            ImportType = importType,
            FileName = fileName,
            ContentHash = hash,
            Status = ImportStatus.Running,
            StartedOnUtc = DateTime.UtcNow,
        };
        db.ImportHistories.Add(history);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var (inserted, updated) = await handler(content, cancellationToken);
            history.InsertedCount = inserted;
            history.UpdatedCount = updated;
            history.Status = ImportStatus.Succeeded;
            history.CompletedOnUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Imported {ImportType} from {Path}: {Inserted} inserted, {Updated} updated.",
                importType, path, inserted, updated);
        }
        catch (Exception ex)
        {
            history.Status = ImportStatus.Failed;
            history.ErrorMessage = ex.Message;
            history.CompletedOnUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            logger.LogError(ex, "Import {ImportType} from {Path} failed.", importType, path);
            throw;
        }
    }

    private async Task<(int Inserted, int Updated)> ImportSurahsAsync(
        string content,
        CancellationToken cancellationToken)
    {
        var records = JsonSerializer.Deserialize<List<SurahSeedRecord>>(content, JsonOptions)
            ?? throw new InvalidOperationException("Surah seed file is not a JSON array.");

        var inserted = 0;
        var updated = 0;

        foreach (var record in records)
        {
            var existing = await db.Surahs
                .FirstOrDefaultAsync(s => s.Number == record.Number, cancellationToken);

            var revelationPlace = Enum.TryParse<RevelationPlace>(record.RevelationPlace, true, out var rp)
                ? rp
                : RevelationPlace.Unknown;

            if (existing is null)
            {
                db.Surahs.Add(new Surah
                {
                    Number = record.Number,
                    NameArabic = record.NameArabic,
                    NameTurkish = record.NameTurkish,
                    NameTransliteration = record.NameTransliteration,
                    VerseCount = record.VerseCount,
                    RevelationPlace = revelationPlace,
                    DisplayOrder = record.DisplayOrder ?? record.Number,
                });
                inserted++;
            }
            else
            {
                existing.NameArabic = record.NameArabic;
                existing.NameTurkish = record.NameTurkish;
                existing.NameTransliteration = record.NameTransliteration;
                existing.VerseCount = record.VerseCount;
                existing.RevelationPlace = revelationPlace;
                existing.DisplayOrder = record.DisplayOrder ?? record.Number;
                updated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return (inserted, updated);
    }

    private async Task<(int Inserted, int Updated)> ImportVersesAsync(
        string content,
        CancellationToken cancellationToken)
    {
        var records = JsonSerializer.Deserialize<List<VerseSeedRecord>>(content, JsonOptions)
            ?? throw new InvalidOperationException("Verses seed file is not a JSON array.");

        var surahMap = await db.Surahs
            .Select(s => new { s.Id, s.Number })
            .ToDictionaryAsync(s => s.Number, s => s.Id, cancellationToken);

        var inserted = 0;
        var updated = 0;

        foreach (var record in records)
        {
            if (!surahMap.TryGetValue(record.SurahNumber, out var surahId))
            {
                throw new InvalidOperationException(
                    $"Cannot import verse {record.SurahNumber}:{record.VerseNumber}; "
                    + "parent surah is missing. Import surahs first.");
            }

            var existing = await db.Verses
                .FirstOrDefaultAsync(
                    v => v.SurahNumber == record.SurahNumber
                        && v.VerseNumber == record.VerseNumber,
                    cancellationToken);

            var cleanArabic = normalizer.Normalize(record.ArabicText);

            if (existing is null)
            {
                db.Verses.Add(new Verse
                {
                    SurahId = surahId,
                    SurahNumber = record.SurahNumber,
                    VerseNumber = record.VerseNumber,
                    GlobalVerseNumber = record.GlobalVerseNumber,
                    JuzNumber = record.JuzNumber,
                    PageNumber = record.PageNumber,
                    ArabicText = record.ArabicText,
                    ArabicTextClean = cleanArabic,
                });
                inserted++;
            }
            else
            {
                existing.GlobalVerseNumber = record.GlobalVerseNumber;
                existing.JuzNumber = record.JuzNumber;
                existing.PageNumber = record.PageNumber;
                existing.ArabicText = record.ArabicText;
                existing.ArabicTextClean = cleanArabic;
                updated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return (inserted, updated);
    }

    private async Task<(int Inserted, int Updated)> ImportTranslationSourcesAsync(
        string content,
        CancellationToken cancellationToken)
    {
        var records = JsonSerializer.Deserialize<List<TranslationSourceSeedRecord>>(content, JsonOptions)
            ?? throw new InvalidOperationException("Translation sources seed file is not a JSON array.");

        var inserted = 0;
        var updated = 0;

        foreach (var record in records)
        {
            var existing = await db.TranslationSources
                .FirstOrDefaultAsync(s => s.Code == record.Code, cancellationToken);

            if (existing is null)
            {
                db.TranslationSources.Add(new TranslationSource
                {
                    Code = record.Code,
                    Name = record.Name,
                    LanguageCode = record.LanguageCode,
                    Author = record.Author,
                    Description = record.Description,
                    LicenseInfo = record.LicenseInfo,
                    SourceUrl = record.SourceUrl,
                    IsDefault = record.IsDefault,
                    IsActive = record.IsActive,
                });
                inserted++;
            }
            else
            {
                existing.Name = record.Name;
                existing.LanguageCode = record.LanguageCode;
                existing.Author = record.Author;
                existing.Description = record.Description;
                existing.LicenseInfo = record.LicenseInfo;
                existing.SourceUrl = record.SourceUrl;
                existing.IsDefault = record.IsDefault;
                existing.IsActive = record.IsActive;
                updated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return (inserted, updated);
    }

    private async Task<(int Inserted, int Updated)> ImportTranslationsAsync(
        string content,
        CancellationToken cancellationToken)
    {
        var file = JsonSerializer.Deserialize<TranslationSeedFile>(content, JsonOptions)
            ?? throw new InvalidOperationException("Translation seed file is malformed.");

        var source = await db.TranslationSources
            .FirstOrDefaultAsync(s => s.Code == file.SourceCode, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Translation source '{file.SourceCode}' is not registered. "
                + "Import translation sources first.");

        var verseLookup = await db.Verses
            .Select(v => new { v.Id, v.SurahNumber, v.VerseNumber })
            .ToDictionaryAsync(v => (v.SurahNumber, v.VerseNumber), v => v.Id, cancellationToken);

        var inserted = 0;
        var updated = 0;

        foreach (var record in file.Translations)
        {
            if (!verseLookup.TryGetValue((record.SurahNumber, record.VerseNumber), out var verseId))
            {
                throw new InvalidOperationException(
                    $"Cannot import translation for {record.SurahNumber}:{record.VerseNumber}; "
                    + "verse is missing. Import verses first.");
            }

            var existing = await db.VerseTranslations
                .FirstOrDefaultAsync(
                    t => t.VerseId == verseId && t.TranslationSourceId == source.Id,
                    cancellationToken);

            var normalized = normalizer.Normalize(record.Text);

            if (existing is null)
            {
                db.VerseTranslations.Add(new VerseTranslation
                {
                    VerseId = verseId,
                    TranslationSourceId = source.Id,
                    Text = record.Text,
                    NormalizedText = normalized,
                });
                inserted++;
            }
            else
            {
                existing.Text = record.Text;
                existing.NormalizedText = normalized;
                updated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return (inserted, updated);
    }

    private static string ComputeHash(string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
