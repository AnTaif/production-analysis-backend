using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/positions")]
public class PositionsController(IDictionariesService dictionariesService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerablePositionDtoExample))]
    [ProducesResponseType<IEnumerable<PositionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositions()
    {
        var dtos = await dictionariesService.GetPositionsAsync();
        return Ok(dtos);
    }
}