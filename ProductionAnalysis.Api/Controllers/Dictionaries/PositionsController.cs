using Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Shared.Constants;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/positions")]
public class PositionsController(
    IDictionariesService dictionariesService,
    IPositionsService positionsService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerablePositionDtoExample))]
    [ProducesResponseType<IEnumerable<PositionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositions()
    {
        var dtos = await dictionariesService.GetPositionsAsync();
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<PositionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PositionDto>> CreatePosition(CreatePositionRequest request)
    {
        var result = await positionsService.CreatePositionAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{positionId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<PositionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PositionDto>> UpdatePosition(
        [FromRoute]
        int positionId,
        UpdatePositionRequest request)
    {
        var result = await positionsService.UpdatePositionAsync(positionId, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{positionId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePosition([FromRoute] int positionId)
    {
        var result = await positionsService.DeletePositionAsync(positionId);
        return result.ToActionResult(this);
    }
}