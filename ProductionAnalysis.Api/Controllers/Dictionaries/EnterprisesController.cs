using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/enterprises")]
public class EnterprisesController(IDictionariesService dictionariesService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableEnterpriseDtoExample))]
    [ProducesResponseType<IEnumerable<EnterpriseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnterprises()
    {
        var dtos = await dictionariesService.GetEnterprisesAsync();
        return Ok(dtos);
    }
}