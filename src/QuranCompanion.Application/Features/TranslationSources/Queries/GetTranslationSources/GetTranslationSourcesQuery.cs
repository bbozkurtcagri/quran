using MediatR;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.TranslationSources.Dtos;

namespace QuranCompanion.Application.Features.TranslationSources.Queries.GetTranslationSources;

public sealed record GetTranslationSourcesQuery(string? LanguageCode = null)
    : IRequest<Result<IReadOnlyList<TranslationSourceDto>>>;
