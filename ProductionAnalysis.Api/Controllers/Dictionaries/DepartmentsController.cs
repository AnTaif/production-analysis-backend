using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/departments")]
public class DepartmentsController(IDictionariesService dictionariesService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableDepartmentDtoExample))]
    [ProducesResponseType<IEnumerable<DepartmentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments()
    {
        var dtos = await dictionariesService.GetDepartmentsAsync();
        return Ok(dtos);
    }
}