using MediatR;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Verses.Dtos;

namespace QuranCompanion.Application.Features.Verses.Queries.GetSurahVerses;

public sealed record GetSurahVersesQuery(
    int SurahNumber,
    string? TranslationSourceCode,
    int Page,
    int PageSize)
    : IRequest<Result<PagedResult<VerseSummaryDto>>>;
