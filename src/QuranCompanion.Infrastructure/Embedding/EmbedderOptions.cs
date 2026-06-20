namespace QuranCompanion.Infrastructure.Embedding;

public sealed class EmbedderOptions
{
    public const string SectionName = "Embedder";

    /// <summary>Base URL of the Python embedder service (e.g. http://embedder:8000).</summary>
    public string BaseUrl { get; set; } = "http://localhost:8001";

    /// <summary>Expected model identifier. Mismatches trigger reindex prompts.</summary>
    public string ModelName { get; set; } = "intfloat/multilingual-e5-small";

    /// <summary>Embedding dimensionality. Must match the schema's vector(N) column.</summary>
    public int Dimension { get; set; } = 384;

    /// <summary>Per-request batch ceiling — must agree with the embedder's EMBEDDER_MAX_BATCH.</summary>
    public int MaxBatch { get; set; } = 64;

    /// <summary>Request timeout for the embedder call.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}
