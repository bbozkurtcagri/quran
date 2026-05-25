using FluentValidation;

namespace QuranCompanion.Application.Features.Verses.Queries.GetVerseByGlobalNumber;

internal sealed class GetVerseByGlobalNumberQueryValidator
    : AbstractValidator<GetVerseByGlobalNumberQuery>
{
    public GetVerseByGlobalNumberQueryValidator()
    {
        RuleFor(x => x.GlobalVerseNumber)
            .InclusiveBetween(1, 6236)
            .WithMessage("Global verse number must be between 1 and 6236.");
    }
}
