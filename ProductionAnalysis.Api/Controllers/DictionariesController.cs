using Core.Auth;
using Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Shared.Constants;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers;

[ApiController]
[Route("dictionaries")]
public class DictionariesController(
    IDictionariesService dictionariesService,
    IEmployeesService employeesService) : ControllerBase
{
    [HttpGet("departments")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableDepartmentDtoExample))]
    [ProducesResponseType<IEnumerable<DepartmentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments()
    {
        var dtos = await dictionariesService.GetDepartmentsAsync();
        return Ok(dtos);
    }

    [HttpGet("downtime-reason-groups")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableDowntimeReasonGroupDtoExample))]
    [ProducesResponseType<IEnumerable<DowntimeReasonGroupDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDowntimeReasonGroups()
    {
        var dtos = await dictionariesService.GetDowntimeReasonGroupsAsync();
        return Ok(dtos);
    }

    [HttpGet("employees")]
    [Authorize]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableEmployeeDtoExample))]
    [ProducesResponseType<IEnumerable<EmployeeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees()
    {
        var user = User.ReadContextUser();
        var dtos = await dictionariesService.GetEmployeesAsync(user);
        return Ok(dtos);
    }

    [HttpGet("enterprises")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableEnterpriseDtoExample))]
    [ProducesResponseType<IEnumerable<EnterpriseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnterprises()
    {
        var dtos = await dictionariesService.GetEnterprisesAsync();
        return Ok(dtos);
    }

    [HttpGet("auxiliary-operations")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableAuxiliaryOperationDtoExample))]
    [ProducesResponseType<IEnumerable<AuxiliaryOperationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuxiliaryOperations()
    {
        var dtos = await dictionariesService.GetAuxiliaryOperationsAsync();
        return Ok(dtos);
    }

    [HttpGet("operations")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableOperationDtoExample))]
    [ProducesResponseType<IEnumerable<OperationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperations()
    {
        var dtos = await dictionariesService.GetOperationsAsync();
        return Ok(dtos);
    }

    [HttpGet("products")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableProductDtoExample))]
    [ProducesResponseType<IEnumerable<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts()
    {
        var dtos = await dictionariesService.GetProductsAsync();
        return Ok(dtos);
    }

    [HttpGet("shifts")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableShiftDtoExample))]
    [ProducesResponseType<IEnumerable<ShiftDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShifts()
    {
        var dtos = await dictionariesService.GetShiftsAsync();
        return Ok(dtos);
    }

    [HttpPost("employees")]
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

    [HttpPut("employees/{employeeId:int}")]
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

    [HttpDelete("employees/{employeeId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteEmployee([FromRoute] int employeeId)
    {
        var result = await employeesService.DeleteEmployeeAsync(employeeId);
        return result.ToActionResult(this);
    }
}