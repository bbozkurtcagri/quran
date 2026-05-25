using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuranCompanion.Api.Common;
using QuranCompanion.Application.Features.Verses.Dtos;
using QuranCompanion.Application.Features.Verses.Queries.GetVerseByGlobalNumber;
using QuranCompanion.Application.Features.Verses.Queries.GetVerseDetail;

namespace QuranCompanion.Api.Controllers;

[ApiController]
[Route("api/v1/verses")]
public sealed class VersesController(ISender sender) : ControllerBase
{
    [HttpGet("{surahNumber:int}/{verseNumber:int}")]
    [ProducesResponseType(typeof(ApiResponse<VerseSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        int surahNumber,
        int verseNumber,
        [FromQuery] string? translationSourceCode,
        CancellationToken cancellationToken)
    {
        var query = new GetVerseDetailQuery(surahNumber, verseNumber, translationSourceCode);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("by-global-number/{globalVerseNumber:int}")]
    [ProducesResponseType(typeof(ApiResponse<VerseSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGlobalNumber(
        int globalVerseNumber,
        [FromQuery] string? translationSourceCode,
        CancellationToken cancellationToken)
    {
        var query = new GetVerseByGlobalNumberQuery(globalVerseNumber, translationSourceCode);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
