using Pgvector;
using QuranCompanion.Domain.Common;
using QuranCompanion.Domain.Quran;

namespace QuranCompanion.Domain.Search;

/// <summary>
/// Semantic search vector for a single (verse, translation source) pair.
/// Each row holds the embedding of one Turkish meal under one embedding
/// model. Re-indexing happens when <see cref="ModelName"/> or
/// <see cref="ContentHash"/> changes, so we can roll the model forward
/// without dropping the table.
/// </summary>
public class VerseEmbedding : BaseEntity
{
    public long VerseId { get; set; }

    public Verse Verse { get; set; } = null!;

    public long TranslationSourceId { get; set; }

    public TranslationSource TranslationSource { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    /// <summary>Hash of the meal text that produced this vector.</summary>
    public string ContentHash { get; set; } = null!;

    public Vector Embedding { get; set; } = null!;
}
