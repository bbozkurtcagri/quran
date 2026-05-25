using MediatR;
using Microsoft.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Application.Common.Errors;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Verses.Dtos;

namespace QuranCompanion.Application.Features.Verses.Queries.GetSurahVerses;

internal sealed class GetSurahVersesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSurahVersesQuery, Result<PagedResult<VerseSummaryDto>>>
{
    public async Task<Result<PagedResult<VerseSummaryDto>>> Handle(
        GetSurahVersesQuery request,
        CancellationToken cancellationToken)
    {
        var surahExists = await db.Surahs
            .AsNoTracking()
            .AnyAsync(s => s.Number == request.SurahNumber, cancellationToken);

        if (!surahExists)
        {
            return Result.Failure<PagedResult<VerseSummaryDto>>(
                Error.NotFound(
                    "surah.not_found",
                    $"Surah with number {request.SurahNumber} was not found."));
        }

        var versesQuery = db.Verses
            .AsNoTracking()
            .Where(v => v.SurahNumber == request.SurahNumber);

        var totalCount = await versesQuery.LongCountAsync(cancellationToken);

        var translationFilter = request.TranslationSourceCode;

        var verses = await versesQuery
            .OrderBy(v => v.VerseNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VerseSummaryDto(
                v.SurahNumber,
                v.VerseNumber,
                v.GlobalVerseNumber,
                v.JuzNumber,
                v.PageNumber,
                v.ArabicText,
                v.Translations
                    .Where(t => translationFilter == null
                        || t.TranslationSource.Code == translationFilter)
                    .OrderBy(t => t.TranslationSource.IsDefault ? 0 : 1)
                    .ThenBy(t => t.TranslationSource.Code)
                    .Select(t => new TranslationDto(
                        t.TranslationSource.Code,
                        t.TranslationSource.Name,
                        t.TranslationSource.LanguageCode,
                        t.TranslationSource.Author,
                        t.Text))
                    .ToList()))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<VerseSummaryDto>(
            verses,
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(result);
    }
}
