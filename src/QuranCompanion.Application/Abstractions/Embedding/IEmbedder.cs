namespace QuranCompanion.Application.Abstractions.Embedding;

/// <summary>
/// Hides the embedding backend behind a single async API. The current
/// implementation calls the Python sidecar; future swaps (managed API, ONNX
/// in-process) keep the same contract.
/// </summary>
public interface IEmbedder
{
    /// <summary>
    /// Identifier of the model that produces the vectors. Persisted alongside
    /// every embedding row so we know whether a regeneration is required.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Dimensionality of every returned vector. Used to validate against the
    /// schema's <c>vector(N)</c> column.
    /// </summary>
    int Dimension { get; }

    /// <summary>
    /// Encodes a batch of texts. <paramref name="kind"/> drives the e5
    /// "query: " vs "passage: " prefix; "query" is used at search time,
    /// "passage" at index time.
    /// </summary>
    Task<IReadOnlyList<float[]>> EmbedAsync(
        IReadOnlyList<string> texts,
        EmbedderKind kind,
        CancellationToken cancellationToken = default);
}

public enum EmbedderKind
{
    Passage = 0,
    Query = 1,
}
