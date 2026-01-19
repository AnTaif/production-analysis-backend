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
[Route("dictionaries/enterprises")]
public class EnterprisesController(
    IDictionariesService dictionariesService,
    IEnterprisesService enterprisesService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableEnterpriseDtoExample))]
    [ProducesResponseType<IEnumerable<EnterpriseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnterprises()
    {
        var dtos = await dictionariesService.GetEnterprisesAsync();
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<EnterpriseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EnterpriseDto>> CreateEnterprise(CreateEnterpriseRequest request)
    {
        var result = await enterprisesService.CreateEnterpriseAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{enterpriseId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<EnterpriseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnterpriseDto>> UpdateEnterprise(
        [FromRoute]
        int enterpriseId,
        UpdateEnterpriseRequest request)
    {
        var result = await enterprisesService.UpdateEnterpriseAsync(enterpriseId, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{enterpriseId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteEnterprise([FromRoute] int enterpriseId)
    {
        var result = await enterprisesService.DeleteEnterpriseAsync(enterpriseId);
        return result.ToActionResult(this);
    }
}