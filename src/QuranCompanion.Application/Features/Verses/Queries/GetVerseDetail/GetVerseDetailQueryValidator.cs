using FluentValidation;

namespace QuranCompanion.Application.Features.Verses.Queries.GetVerseDetail;

internal sealed class GetVerseDetailQueryValidator : AbstractValidator<GetVerseDetailQuery>
{
    public GetVerseDetailQueryValidator()
    {
        RuleFor(x => x.SurahNumber)
            .InclusiveBetween(1, 114)
            .WithMessage("Surah number must be between 1 and 114.");

        RuleFor(x => x.VerseNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Verse number must be 1 or greater.");
    }
}
