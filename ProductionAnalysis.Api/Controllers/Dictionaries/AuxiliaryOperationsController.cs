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
[Route("dictionaries/auxiliary-operations")]
public class AuxiliaryOperationsController(
    IDictionariesService dictionariesService,
    IAuxiliaryOperationsService auxiliaryOperationsService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableAuxiliaryOperationDtoExample))]
    [ProducesResponseType<IEnumerable<AuxiliaryOperationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuxiliaryOperations()
    {
        var dtos = await dictionariesService.GetAuxiliaryOperationsAsync();
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<AuxiliaryOperationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuxiliaryOperationDto>> CreateAuxiliaryOperation(
        CreateAuxiliaryOperationRequest request)
    {
        var result = await auxiliaryOperationsService.CreateAuxiliaryOperationAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<AuxiliaryOperationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuxiliaryOperationDto>> UpdateAuxiliaryOperation(
        [FromRoute]
        int id,
        UpdateAuxiliaryOperationRequest request)
    {
        var result = await auxiliaryOperationsService.UpdateAuxiliaryOperationAsync(id, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAuxiliaryOperation([FromRoute] int id)
    {
        var result = await auxiliaryOperationsService.DeleteAuxiliaryOperationAsync(id);
        return result.ToActionResult(this);
    }
}