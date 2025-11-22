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
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableEmployeeDtoExample))]
    [ProducesResponseType<IEnumerable<EmployeeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees()
    {
        var dtos = await dictionariesService.GetEmployeesAsync();
        return Ok(dtos);
    }

    [HttpGet("enterprises")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableEnterpriseDtoExample))]
    [ProducesResponseType<IEnumerable<EnterpriseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnterprises()
    {
        var dtos = await dictionariesService.GetEnterpriseAsync();
        return Ok(dtos);
    }

    [HttpGet("additional-operations")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableAdditionalOperationDtoExample))]
    [ProducesResponseType<IEnumerable<AdditionalOperationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdditionalOperations()
    {
        var dtos = await dictionariesService.GetAdditionalOperationsAsync();
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

    [HttpGet("pa-types")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerablePaTypeDtoExample))]
    [ProducesResponseType<IEnumerable<PaTypeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaTypes()
    {
        var dtos = await dictionariesService.GetPaTypesAsync();
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
}