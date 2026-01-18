using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/shifts")]
public class ShiftsController(IDictionariesService dictionariesService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableShiftDtoExample))]
    [ProducesResponseType<IEnumerable<ShiftDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShifts()
    {
        var dtos = await dictionariesService.GetShiftsAsync();
        return Ok(dtos);
    }
}