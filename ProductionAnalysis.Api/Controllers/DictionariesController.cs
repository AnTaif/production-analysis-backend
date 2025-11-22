using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers;

[ApiController]
[Route("dictionaries")]
public class DictionariesController(IDictionariesService dictionariesService) : ControllerBase
{
    [HttpGet("departments")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DepartmentDtoExample))]
    [ProducesResponseType<DepartmentDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments()
    {
        var dtos = await dictionariesService.GetDepartmentsAsync();
        return Ok(dtos);
    }
    
    [HttpGet("downtime-reason-groups")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DowntimeReasonGroupDtoExample))]
    [ProducesResponseType<DowntimeReasonGroupDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDowntimeReasonGroups()
    {
        var dtos = await dictionariesService.GetDowntimeReasonGroupsAsync();
        return Ok(dtos);
    }

    [HttpGet("employees")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EmployeeDtoExample))]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees()
    {
        var dtos = await dictionariesService.GetEmployeesAsync();
        return Ok(dtos);
    }

    [HttpGet("enterprises")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnterpriseDtoExample))]
    [ProducesResponseType<EnterpriseDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnterprises()
    {
        var dtos = await dictionariesService.GetEnterpriseAsync();
        return Ok(dtos);
    }

    [HttpGet("operations")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(OperationDtoExample))]
    [ProducesResponseType<OperationDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperations()
    {
        var dtos = await dictionariesService.GetOperationsAsync();
        return Ok(dtos);
    }

    [HttpGet("pa-types")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PaTypeDtoExample))]
    [ProducesResponseType<PaTypeDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaTypes()
    {
        var dtos = await dictionariesService.GetPaTypesAsync();
        return Ok(dtos);
    }

    [HttpGet("shifts")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ShiftDtoExample))]
    [ProducesResponseType<ShiftDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShifts()
    {
        var dtos = await dictionariesService.GetShiftsAsync();
        return Ok(dtos);
    }
}