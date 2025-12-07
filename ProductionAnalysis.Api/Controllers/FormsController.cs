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
    public async Task<ActionResult<PaginatedResponse<FormShortDto>>> SearchForms(SearchFormsFilterDto searchFormsFilter)
    {
        var result = await formsService.SearchFormsAsync(searchFormsFilter);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType<FormShortDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FormShortDto>> CreateNewForm(CreateFormRequest createFormRequest)
    {
        var userId = User.ReadSid();
        var result = await formsService.CreateAsync(createFormRequest, userId);
        return result.ToActionResult(this);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<FormDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormDto>> GetFormById(Guid id)
    {
        throw new NotImplementedException();
    }
}