using MediatR;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Surahs.Dtos;

namespace QuranCompanion.Application.Features.Surahs.Queries.GetSurahDetail;

public sealed record GetSurahDetailQuery(int SurahNumber) : IRequest<Result<SurahDetailDto>>;
