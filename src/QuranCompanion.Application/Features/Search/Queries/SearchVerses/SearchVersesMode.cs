namespace QuranCompanion.Application.Features.Search.Queries.SearchVerses;

public enum SearchVersesMode
{
    /// <summary>Trigram + ILIKE over the normalised meal text (Phase 1 default).</summary>
    Keyword = 0,

    /// <summary>Cosine similarity over the meal embeddings (Phase 2, requires the embedder).</summary>
    Semantic = 1,
}
