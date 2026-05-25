using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuranCompanion.Api.Common;
using QuranCompanion.Application.Common.Models;
using QuranCompanion.Application.Features.Search.Dtos;
using QuranCompanion.Application.Features.Search.Queries.SearchVerses;

namespace QuranCompanion.Api.Controllers;

[ApiController]
[Route("api/v1/search")]
public sealed class SearchController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VerseSearchHitDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "query")] string query,
        [FromQuery] string? translationSourceCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new SearchVersesQuery(query ?? string.Empty, translationSourceCode, page, pageSize);
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
