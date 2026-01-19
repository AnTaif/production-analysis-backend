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
[Route("dictionaries/downtime-reason-groups")]
public class DowntimeReasonGroupsController(
    IDictionariesService dictionariesService,
    IDowntimeReasonGroupsService downtimeReasonGroupsService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableDowntimeReasonGroupDtoExample))]
    [ProducesResponseType<IEnumerable<DowntimeReasonGroupDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDowntimeReasonGroups()
    {
        var dtos = await dictionariesService.GetDowntimeReasonGroupsAsync();
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<DowntimeReasonGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DowntimeReasonGroupDto>> CreateDowntimeReasonGroup(
        CreateDowntimeReasonGroupRequest request)
    {
        var result = await downtimeReasonGroupsService.CreateDowntimeReasonGroupAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<DowntimeReasonGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DowntimeReasonGroupDto>> UpdateDowntimeReasonGroup(
        [FromRoute]
        int id,
        UpdateDowntimeReasonGroupRequest request)
    {
        var result = await downtimeReasonGroupsService.UpdateDowntimeReasonGroupAsync(id, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDowntimeReasonGroup([FromRoute] int id)
    {
        var result = await downtimeReasonGroupsService.DeleteDowntimeReasonGroupAsync(id);
        return result.ToActionResult(this);
    }
}