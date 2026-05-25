using MediatR;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Verses.Dtos;

namespace QuranCompanion.Application.Features.Verses.Queries.GetVerseByGlobalNumber;

public sealed record GetVerseByGlobalNumberQuery(
    int GlobalVerseNumber,
    string? TranslationSourceCode)
    : IRequest<Result<VerseSummaryDto>>;
