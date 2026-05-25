using FluentValidation;
using QuranCompanion.Application.Common.Models;

namespace QuranCompanion.Application.Features.Verses.Queries.GetSurahVerses;

internal sealed class GetSurahVersesQueryValidator : AbstractValidator<GetSurahVersesQuery>
{
    public GetSurahVersesQueryValidator()
    {
        RuleFor(x => x.SurahNumber)
            .InclusiveBetween(1, 114)
            .WithMessage("Surah number must be between 1 and 114.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PageRequest.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {PageRequest.MaxPageSize}.");

        RuleFor(x => x.TranslationSourceCode)
            .MaximumLength(64)
            .When(x => !string.IsNullOrEmpty(x.TranslationSourceCode));
    }
}
