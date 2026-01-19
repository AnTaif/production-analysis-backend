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
[Route("dictionaries/departments")]
public class DepartmentsController(
    IDictionariesService dictionariesService,
    IDepartmentsService departmentsService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableDepartmentDtoExample))]
    [ProducesResponseType<IEnumerable<DepartmentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments()
    {
        var dtos = await dictionariesService.GetDepartmentsAsync();
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<DepartmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentRequest request)
    {
        var result = await departmentsService.CreateDepartmentAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{departmentId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<DepartmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(
        [FromRoute]
        int departmentId,
        UpdateDepartmentRequest request)
    {
        var result = await departmentsService.UpdateDepartmentAsync(departmentId, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{departmentId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDepartment([FromRoute] int departmentId)
    {
        var result = await departmentsService.DeleteDepartmentAsync(departmentId);
        return result.ToActionResult(this);
    }
}