using MediatR;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Search.Dtos;

namespace QuranCompanion.Application.Features.Search.Queries.SearchVerses;

public sealed record SearchVersesQuery(
    string Query,
    string? TranslationSourceCode,
    int Page,
    int PageSize)
    : IRequest<Result<PagedResult<VerseSearchHitDto>>>;
