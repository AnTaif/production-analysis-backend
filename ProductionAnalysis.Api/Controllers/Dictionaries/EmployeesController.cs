using Core.Auth;
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
[Route("dictionaries/employees")]
public class EmployeesController(
    IDictionariesService dictionariesService,
    IEmployeesService employeesService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableEmployeeDtoExample))]
    [ProducesResponseType<IEnumerable<EmployeeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees()
    {
        var user = User.ReadContextUser();
        var dtos = await dictionariesService.GetEmployeesAsync(user);
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [SwaggerRequestExample(typeof(CreateEmployeeRequest), typeof(CreateEmployeeRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EmployeeDtoExample))]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeRequest request)
    {
        var result = await employeesService.CreateEmployeeAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{employeeId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [SwaggerRequestExample(typeof(UpdateEmployeeRequest), typeof(UpdateEmployeeRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EmployeeDtoExample))]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> UpdateEmployee(
        [FromRoute]
        int employeeId,
        UpdateEmployeeRequest request)
    {
        var result = await employeesService.UpdateEmployeeAsync(employeeId, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{employeeId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteEmployee([FromRoute] int employeeId)
    {
        var result = await employeesService.DeleteEmployeeAsync(employeeId);
        return result.ToActionResult(this);
    }
}