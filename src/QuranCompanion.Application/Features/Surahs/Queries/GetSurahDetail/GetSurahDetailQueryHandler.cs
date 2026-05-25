using MediatR;
using Microsoft.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Application.Common.Errors;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Surahs.Dtos;

namespace QuranCompanion.Application.Features.Surahs.Queries.GetSurahDetail;

internal sealed class GetSurahDetailQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSurahDetailQuery, Result<SurahDetailDto>>
{
    public async Task<Result<SurahDetailDto>> Handle(
        GetSurahDetailQuery request,
        CancellationToken cancellationToken)
    {
        var surah = await db.Surahs
            .AsNoTracking()
            .Where(s => s.Number == request.SurahNumber)
            .Select(s => new SurahDetailDto(
                s.Number,
                s.NameArabic,
                s.NameTurkish,
                s.NameTransliteration,
                s.VerseCount,
                s.RevelationPlace.ToString(),
                s.DisplayOrder))
            .FirstOrDefaultAsync(cancellationToken);

        if (surah is null)
        {
            return Result.Failure<SurahDetailDto>(
                Error.NotFound(
                    "surah.not_found",
                    $"Surah with number {request.SurahNumber} was not found."));
        }

        return Result.Success(surah);
    }
}
