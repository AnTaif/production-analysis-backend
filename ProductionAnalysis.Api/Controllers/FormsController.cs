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
    [SwaggerRequestExample(typeof(SearchFormsFilterDto), typeof(SearchFormsFilterDtoExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PaginatedFormShortDtoExample))]
    [ProducesResponseType<PaginatedResponse<FormShortDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<FormShortDto>>> SearchForms(SearchFormsFilterDto searchFormsFilter)
    {
        var user = User.ReadContextUser();
        var result = await formsService.SearchFormsAsync(searchFormsFilter, user);
        return result.ToActionResult(this);
    }

    /// <remarks>
    /// Для вызова метода у пользователя должна быть роль DepartmentHead
    /// </remarks>
    /// <param name="createFormRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = Roles.DepartmentHead)]
    [SwaggerRequestExample(typeof(CreateFormRequest), typeof(CreateFormRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(FormShortDtoExample))]
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
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(FormDtoExample))]
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
    [SwaggerRequestExample(typeof(UpdateFormRowRequest), typeof(UpdateFormRowRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateFormRowResponseExample))]
    [ProducesResponseType<UpdateFormRowResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UpdateFormRowResponse>> UpdateFormRow(int formId, short rowOrder,
        UpdateFormRowRequest request)
    {
        var userId = User.ReadSid();
        var result = await formsService.UpdateFormRowAsync(formId, rowOrder, request, userId);
        return result.ToActionResult(this);
    }

    [HttpPost("{formId:int}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CompleteForm([Range(1, int.MaxValue)] int formId)
    {
        var userId = User.ReadSid();
        var result = await formsService.CompleteFormAsync(formId, userId);
        return result.ToActionResult(this);
    }
}