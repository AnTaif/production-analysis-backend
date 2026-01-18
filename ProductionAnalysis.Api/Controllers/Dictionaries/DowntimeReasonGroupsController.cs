using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/downtime-reason-groups")]
public class DowntimeReasonGroupsController(IDictionariesService dictionariesService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableDowntimeReasonGroupDtoExample))]
    [ProducesResponseType<IEnumerable<DowntimeReasonGroupDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDowntimeReasonGroups()
    {
        var dtos = await dictionariesService.GetDowntimeReasonGroupsAsync();
        return Ok(dtos);
    }
}