using System.ComponentModel.DataAnnotations;
using Core.Auth;
using Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Forms;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Client.Models.Forms;
using Shared.Constants;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers;

[ApiController]
[Route("forms")]
[Authorize]
public class FormsController(IFormsService formsService) : ControllerBase
{
    [HttpPost("search")]
    [ProducesResponseType<PaginatedResponse<FormShortDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<FormShortDto>>> SearchForms(SearchFormsFilterDto searchFormsFilter)
    {
        var result = await formsService.SearchFormsAsync(searchFormsFilter);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Roles = Roles.DepartmentHead)]
    [SwaggerRequestExample(typeof(CreateFormRequest), typeof(CreateFormRequestExample))]
    [ProducesResponseType<FormShortDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FormShortDto>> CreateNewForm(CreateFormRequest createFormRequest)
    {
        var userId = User.ReadSid();
        var result = await formsService.CreateAsync(createFormRequest, userId);

        return result.ToActionResult(this, dto => CreatedAtAction(
            nameof(GetFormById),
            new { id = dto.Id },
            dto));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<FormDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FormDto>> GetFormById([Range(1, int.MaxValue)] int id)
    {
        var result = await formsService.GetByIdAsync(id);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:int}/rows")]
    [ProducesResponseType<ICollection<FormRowDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ICollection<FormRowDto>>> GetFormRows([Range(1, int.MaxValue)] int id)
    {
        var result = await formsService.GetFormRowsAsync(id);
        return result.ToActionResult(this);
    }

    [HttpPut("{formId:int}/rows/{rowOrder}")]
    [ProducesResponseType<FormRowDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FormRowDto>> UpdateFormRow(int formId, short rowOrder, UpdateFormRowRequest request)
    {
        var userId = User.ReadSid();
        var result = await formsService.UpdateFormRowAsync(formId, rowOrder, request, userId);
        return result.ToActionResult(this);
    }
}