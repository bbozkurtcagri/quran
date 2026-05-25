using MediatR;
using Microsoft.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Surahs.Dtos;

namespace QuranCompanion.Application.Features.Surahs.Queries.GetSurahs;

internal sealed class GetSurahsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSurahsQuery, Result<IReadOnlyList<SurahListItemDto>>>
{
    public async Task<Result<IReadOnlyList<SurahListItemDto>>> Handle(
        GetSurahsQuery request,
        CancellationToken cancellationToken)
    {
        var surahs = await db.Surahs
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Number)
            .Select(s => new SurahListItemDto(
                s.Number,
                s.NameArabic,
                s.NameTurkish,
                s.NameTransliteration,
                s.VerseCount,
                s.RevelationPlace.ToString()))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SurahListItemDto>>(surahs);
    }
}
