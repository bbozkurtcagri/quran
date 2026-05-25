using FluentValidation;

namespace QuranCompanion.Application.Features.Surahs.Queries.GetSurahDetail;

internal sealed class GetSurahDetailQueryValidator : AbstractValidator<GetSurahDetailQuery>
{
    public GetSurahDetailQueryValidator()
    {
        RuleFor(x => x.SurahNumber)
            .InclusiveBetween(1, 114)
            .WithMessage("Surah number must be between 1 and 114.");
    }
}
