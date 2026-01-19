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
[Route("dictionaries/shifts")]
public class ShiftsController(
    IDictionariesService dictionariesService,
    IShiftsService shiftsService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableShiftDtoExample))]
    [ProducesResponseType<IEnumerable<ShiftDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShifts()
    {
        var dtos = await dictionariesService.GetShiftsAsync();
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<ShiftDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShiftDto>> CreateShift(CreateShiftRequest request)
    {
        var result = await shiftsService.CreateShiftAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{shiftId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<ShiftDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShiftDto>> UpdateShift(
        [FromRoute]
        int shiftId,
        UpdateShiftRequest request)
    {
        var result = await shiftsService.UpdateShiftAsync(shiftId, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{shiftId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteShift([FromRoute] int shiftId)
    {
        var result = await shiftsService.DeleteShiftAsync(shiftId);
        return result.ToActionResult(this);
    }
}