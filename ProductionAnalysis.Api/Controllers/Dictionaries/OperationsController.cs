using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/operations")]
public class OperationsController(IDictionariesService dictionariesService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableOperationDtoExample))]
    [ProducesResponseType<IEnumerable<OperationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperations()
    {
        var dtos = await dictionariesService.GetOperationsAsync();
        return Ok(dtos);
    }
}