using MediatR;
using Microsoft.EntityFrameworkCore;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.TranslationSources.Dtos;

namespace QuranCompanion.Application.Features.TranslationSources.Queries.GetTranslationSources;

internal sealed class GetTranslationSourcesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTranslationSourcesQuery, Result<IReadOnlyList<TranslationSourceDto>>>
{
    public async Task<Result<IReadOnlyList<TranslationSourceDto>>> Handle(
        GetTranslationSourcesQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.TranslationSources
            .AsNoTracking()
            .Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(request.LanguageCode))
        {
            query = query.Where(s => s.LanguageCode == request.LanguageCode);
        }

        var sources = await query
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.LanguageCode)
            .ThenBy(s => s.Name)
            .Select(s => new TranslationSourceDto(
                s.Code,
                s.Name,
                s.LanguageCode,
                s.Author,
                s.Description,
                s.LicenseInfo,
                s.SourceUrl,
                s.IsDefault))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TranslationSourceDto>>(sources);
    }
}
