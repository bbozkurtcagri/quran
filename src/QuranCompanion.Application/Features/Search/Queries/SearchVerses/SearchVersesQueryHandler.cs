using MediatR;
using Microsoft.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Application.Abstractions.Text;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Search.Dtos;

namespace QuranCompanion.Application.Features.Search.Queries.SearchVerses;

internal sealed class SearchVersesQueryHandler(
    IApplicationDbContext db,
    ITextNormalizer normalizer)
    : IRequestHandler<SearchVersesQuery, Result<PagedResult<VerseSearchHitDto>>>
{
    public async Task<Result<PagedResult<VerseSearchHitDto>>> Handle(
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
}
