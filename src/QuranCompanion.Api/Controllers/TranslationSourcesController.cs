using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using QuranCompanion.Api.Common;
using QuranCompanion.Application.Features.TranslationSources.Dtos;
using QuranCompanion.Application.Features.TranslationSources.Queries.GetTranslationSources;

namespace QuranCompanion.Api.Controllers;

[ApiController]
[Route("api/v1/translation-sources")]
[CacheControl(CachePolicies.TranslationSources)]
[OutputCache(PolicyName = "TranslationSources")]
public sealed class TranslationSourcesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TranslationSourceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? languageCode,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTranslationSourcesQuery(languageCode), cancellationToken);
        return result.ToActionResult();
    }
}
