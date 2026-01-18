using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/auxiliary-operations")]
public class AuxiliaryOperationsController(IDictionariesService dictionariesService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableAuxiliaryOperationDtoExample))]
    [ProducesResponseType<IEnumerable<AuxiliaryOperationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuxiliaryOperations()
    {
        var dtos = await dictionariesService.GetAuxiliaryOperationsAsync();
        return Ok(dtos);
    }
}