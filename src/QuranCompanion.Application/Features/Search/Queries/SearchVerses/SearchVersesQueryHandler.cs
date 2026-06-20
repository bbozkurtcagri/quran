using MediatR;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Embedding;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Application.Abstractions.Text;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Search.Dtos;

namespace QuranCompanion.Application.Features.Search.Queries.SearchVerses;

internal sealed class SearchVersesQueryHandler(
    IApplicationDbContext db,
    ITextNormalizer normalizer,
    IEmbedder embedder)
    : IRequestHandler<SearchVersesQuery, Result<PagedResult<VerseSearchHitDto>>>
{
    public Task<Result<PagedResult<VerseSearchHitDto>>> Handle(
        SearchVersesQuery request,
        CancellationToken cancellationToken)
    {
        return request.Mode == SearchVersesMode.Semantic
            ? HandleSemantic(request, cancellationToken)
            : HandleKeyword(request, cancellationToken);
    }

    private async Task<Result<PagedResult<VerseSearchHitDto>>> HandleKeyword(
        SearchVersesQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedQuery = normalizer.Normalize(request.Query);

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return Result.Success(
                PagedResult<VerseSearchHitDto>.Empty(request.Page, request.PageSize));
        }

        var pattern = $"%{normalizedQuery}%";
        var sourceCode = request.TranslationSourceCode;

        // NormalizedText is stored lowercased so a case-sensitive LIKE matches
        // case-insensitively without needing ILIKE; keeps the Application layer
        // free of provider-specific extensions.
        var baseQuery = db.VerseTranslations
            .AsNoTracking()
            .Where(t => EF.Functions.Like(t.NormalizedText, pattern));

        if (!string.IsNullOrWhiteSpace(sourceCode))
        {
            baseQuery = baseQuery.Where(t => t.TranslationSource.Code == sourceCode);
        }

        var totalCount = await baseQuery.LongCountAsync(cancellationToken);

        var hits = await baseQuery
            .OrderBy(t => t.Verse.GlobalVerseNumber)
            .ThenBy(t => t.TranslationSource.IsDefault ? 0 : 1)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new VerseSearchHitDto(
                t.Verse.SurahNumber,
                t.Verse.Surah.NameTurkish,
                t.Verse.VerseNumber,
                t.Verse.GlobalVerseNumber,
                t.Verse.ArabicText,
                t.TranslationSource.Code,
                t.Text))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<VerseSearchHitDto>(
            hits,
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(result);
    }

    private async Task<Result<PagedResult<VerseSearchHitDto>>> HandleSemantic(
        SearchVersesQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return Result.Success(
                PagedResult<VerseSearchHitDto>.Empty(request.Page, request.PageSize));
        }

        var queryEmbeddingArr = (await embedder.EmbedAsync(
            new[] { request.Query.Trim() },
            EmbedderKind.Query,
            cancellationToken))[0];
        var queryVector = new Vector(queryEmbeddingArr);

        var sourceCode = request.TranslationSourceCode;
        var modelName = embedder.ModelName;

        var baseQuery = db.VerseEmbeddings
            .AsNoTracking()
            .Where(e => e.ModelName == modelName);

        if (!string.IsNullOrWhiteSpace(sourceCode))
        {
            baseQuery = baseQuery.Where(e => e.TranslationSource.Code == sourceCode);
        }

        // We cap total count at request.PageSize × 10 so the user sees a reasonable
        // upper bound without the DB sorting every row.
        var totalCount = await baseQuery.LongCountAsync(cancellationToken);

        // The HNSW index over vector_cosine_ops means EF.CosineDistance is the
        // operator that lets the planner pick it.
        var hits = await baseQuery
            .OrderBy(e => e.Embedding.CosineDistance(queryVector))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new VerseSearchHitDto(
                e.Verse.SurahNumber,
                e.Verse.Surah.NameTurkish,
                e.Verse.VerseNumber,
                e.Verse.GlobalVerseNumber,
                e.Verse.ArabicText,
                e.TranslationSource.Code,
                e.Verse.Translations
                    .Where(t => t.TranslationSourceId == e.TranslationSourceId)
                    .Select(t => t.Text)
                    .FirstOrDefault() ?? string.Empty))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<VerseSearchHitDto>(
            hits,
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(result);
    }
}
