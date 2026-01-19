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
[Route("dictionaries/operations")]
public class OperationsController(
    IDictionariesService dictionariesService,
    IOperationsService operationsService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableOperationDtoExample))]
    [ProducesResponseType<IEnumerable<OperationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperations()
    {
        var dtos = await dictionariesService.GetOperationsAsync();
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<OperationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationDto>> CreateOperation(CreateOperationRequest request)
    {
        var result = await operationsService.CreateOperationAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{operationId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<OperationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationDto>> UpdateOperation(
        [FromRoute]
        int operationId,
        UpdateOperationRequest request)
    {
        var result = await operationsService.UpdateOperationAsync(operationId, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{operationId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteOperation([FromRoute] int operationId)
    {
        var result = await operationsService.DeleteOperationAsync(operationId);
        return result.ToActionResult(this);
    }
}