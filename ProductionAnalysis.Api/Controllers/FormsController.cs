using Core.Auth;
using Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Client.Models.Forms;

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
    public async Task<ActionResult<FormDto>> GetFormById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Form ID must be greater than zero.");
        }

        var result = await formsService.GetByIdAsync(id);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:int}/rows")]
    [ProducesResponseType<ICollection<FormRowDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ICollection<FormRowDto>>> GetFormRows(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Form ID must be greater than zero.");
        }

        var result = await formsService.GetFormRowsAsync(id);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:int}/rows/{rowOrder}")]
    [ProducesResponseType<FormRowDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FormRowDto>> UpdateFormRow(int id, short rowOrder, UpdateFormRowRequest request)
    {
        if (id <= 0)
        {
            return BadRequest("Form ID must be greater than zero.");
        }

        if (rowOrder <= 0)
        {
            return BadRequest("Row order must be greater than zero.");
        }

        if (request.FormId != id)
        {
            return BadRequest("Form ID in request body must match the ID in URL.");
        }

        if (request.RowOrder != rowOrder)
        {
            return BadRequest("Row order in request body must match the row order in URL.");
        }

        var userId = User.ReadSid();
        var result = await formsService.UpdateFormRowAsync(request, userId);
        return result.ToActionResult(this);
    }
}