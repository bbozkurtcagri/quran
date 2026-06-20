using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using QuranCompanion.Application.Abstractions.Embedding;

namespace QuranCompanion.Infrastructure.Embedding;

internal sealed class HttpEmbedder(HttpClient http, IOptions<EmbedderOptions> options) : IEmbedder
{
    private readonly EmbedderOptions _options = options.Value;

    public string ModelName => _options.ModelName;

    public int Dimension => _options.Dimension;

    public async Task<IReadOnlyList<float[]>> EmbedAsync(
        IReadOnlyList<string> texts,
        EmbedderKind kind,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0)
        {
            return Array.Empty<float[]>();
        }
        if (texts.Count > _options.MaxBatch)
        {
            throw new ArgumentException(
                $"Batch size {texts.Count} exceeds embedder limit {_options.MaxBatch}.",
                nameof(texts));
        }

        var request = new EmbedRequest(
            texts.ToArray(),
            kind == EmbedderKind.Query ? "query" : "passage");

        using var response = await http.PostAsJsonAsync(
            "/embed",
            request,
            EmbedderJsonContext.Default.EmbedRequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync(
            EmbedderJsonContext.Default.EmbedResponse,
            cancellationToken)
            ?? throw new InvalidOperationException("Embedder returned an empty response.");

        if (payload.Embeddings.Count != texts.Count)
        {
            throw new InvalidOperationException(
                $"Embedder returned {payload.Embeddings.Count} vectors for {texts.Count} inputs.");
        }

        if (payload.Dimension != _options.Dimension)
        {
            throw new InvalidOperationException(
                $"Embedder dimension {payload.Dimension} does not match expected {_options.Dimension}. " +
                $"Reindex required or adjust {nameof(EmbedderOptions)}.");
        }

        return payload.Embeddings;
    }
}

internal sealed record EmbedRequest(
    [property: JsonPropertyName("texts")] string[] Texts,
    [property: JsonPropertyName("kind")] string Kind);

internal sealed record EmbedResponse(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("dimension")] int Dimension,
    [property: JsonPropertyName("embeddings")] IReadOnlyList<float[]> Embeddings);

[JsonSerializable(typeof(EmbedRequest))]
[JsonSerializable(typeof(EmbedResponse))]
internal sealed partial class EmbedderJsonContext : JsonSerializerContext
{
}
