using FluentValidation;
using QuranCompanion.Application.Common.Models;

namespace QuranCompanion.Application.Features.Search.Queries.SearchVerses;

internal sealed class SearchVersesQueryValidator : AbstractValidator<SearchVersesQuery>
{
    public SearchVersesQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Query is required.")
            .MinimumLength(2)
            .WithMessage("Query must be at least 2 characters.")
            .MaximumLength(256)
            .WithMessage("Query must be at most 256 characters.");

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
