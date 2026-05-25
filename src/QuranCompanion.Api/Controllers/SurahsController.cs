using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuranCompanion.Api.Common;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Surahs.Dtos;
using QuranCompanion.Application.Features.Surahs.Queries.GetSurahDetail;
using QuranCompanion.Application.Features.Surahs.Queries.GetSurahs;
using QuranCompanion.Application.Features.Verses.Dtos;
using QuranCompanion.Application.Features.Verses.Queries.GetSurahVerses;

namespace QuranCompanion.Api.Controllers;

[ApiController]
[Route("api/v1/surahs")]
public sealed class SurahsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SurahListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSurahsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{surahNumber:int}")]
    [ProducesResponseType(typeof(ApiResponse<SurahDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int surahNumber, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSurahDetailQuery(surahNumber), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{surahNumber:int}/verses")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VerseSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verses(
        int surahNumber,
        [FromQuery] string? translationSourceCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSurahVersesQuery(surahNumber, translationSourceCode, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
