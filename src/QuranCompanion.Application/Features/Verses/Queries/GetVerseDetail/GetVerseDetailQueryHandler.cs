using MediatR;
using Microsoft.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Application.Common.Errors;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Verses.Dtos;

namespace QuranCompanion.Application.Features.Verses.Queries.GetVerseDetail;

internal sealed class GetVerseDetailQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetVerseDetailQuery, Result<VerseSummaryDto>>
{
    public async Task<Result<VerseSummaryDto>> Handle(
        GetVerseDetailQuery request,
        CancellationToken cancellationToken)
    {
        var translationFilter = request.TranslationSourceCode;

        var verse = await db.Verses
            .AsNoTracking()
            .Where(v => v.SurahNumber == request.SurahNumber
                && v.VerseNumber == request.VerseNumber)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (verse is null)
        {
            return Result.Failure<VerseSummaryDto>(
                Error.NotFound(
                    "verse.not_found",
                    $"Verse {request.SurahNumber}:{request.VerseNumber} was not found."));
        }

        return Result.Success(verse);
    }
}
