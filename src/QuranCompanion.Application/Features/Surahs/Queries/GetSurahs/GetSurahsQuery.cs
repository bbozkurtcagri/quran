using MediatR;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Surahs.Dtos;

namespace QuranCompanion.Application.Features.Surahs.Queries.GetSurahs;

public sealed record GetSurahsQuery : IRequest<Result<IReadOnlyList<SurahListItemDto>>>;
