using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormsService
{
    Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter);
    Task<Result<FormShortDto>> CreateAsync(CreateFormRequest request, Guid creatorId);
    Task<Result<FormDto>> GetByIdAsync(int formId);
    Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId);
    Task<Result<FormRowDto>> UpdateFormRowAsync(UpdateFormRowRequest request, Guid userId);
}

[RegisterScoped]
public class FormsService(
    IPaUnitOfWork unitOfWork,
    IFormRowGenerator formRowGenerator,
    IFormRowUpdateValidator formRowUpdateValidator,
    IFormRowValueFilter formRowValueFilter,
    IFormRowFormulaCalculator formRowFormulaCalculator
)
    : IFormsService
{
    public async Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter)
    {
        var domainFilter = searchFilter.ToDomain();
        var (forms, totalCount) = await unitOfWork.Forms.SearchFormsAsync(domainFilter);

        var dtos = forms.Select(f => f.ToShortDto()).ToList();

        var response = new PaginatedResponse<FormShortDto>(
            dtos,
            totalCount,
            domainFilter.PageNumber,
            domainFilter.PageSize
        );

        return response;
    }

    public async Task<Result<FormShortDto>> CreateAsync(CreateFormRequest request, Guid creatorId)
    {
        var createForm = request.ToDomain(creatorId);

        var template = await unitOfWork.Templates.GetLatestByPaTypeIdAsync(createForm.PaTypeId);
        if (template == null)
        {
            return ServiceError.NotFound($"Template for PaType {createForm.PaTypeId} not found");
        }

        createForm.TemplateSnapshot = TemplateSerializer.SerializeTemplateSnapshot(template);

        var form = await unitOfWork.Forms.CreateAsync(createForm);

        await CreateFormRowsIfNeededAsync(form.Id, createForm.ShiftId, template);
        await unitOfWork.SaveChangesAsync();

        return form.ToShortDto();
    }

    public async Task<Result<FormDto>> GetByIdAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {formId} not found");
        }

        return form.ToDto();
    }

    public async Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {formId} not found");
        }

        return form.Rows.ToRowDtos();
    }

    private async Task CreateFormRowsIfNeededAsync(int formId, int shiftId, Template template)
    {
        var shift = await unitOfWork.Dictionaries.SelectShiftByIdAsync(shiftId);
        if (shift == null)
        {
            return;
        }

        var schedules = await unitOfWork.Dictionaries.SelectShiftSchedulesByShiftIdAsync(shiftId);
        var rows = await formRowGenerator.GenerateRowsForShiftAsync(
            shift.StartTime,
            schedules,
            template);

        await unitOfWork.Forms.CreateFormRowsAsync(formId, rows);
    }

    public async Task<Result<FormRowDto>> UpdateFormRowAsync(UpdateFormRowRequest request, Guid userId)
    {
        var form = await unitOfWork.Forms.FindAsync(request.FormId);
        var validationResult = formRowUpdateValidator.Validate(request, form);
        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        var (validatedForm, validatedRow) = validationResult.Value;

        var filteredValues = formRowValueFilter.FilterUpdatableValues(
            request.Values,
            validatedForm.TemplateSnapshot);

        if (filteredValues.Count == 0)
        {
            return validatedRow.ToRowDto();
        }

        var template = await unitOfWork.Templates.GetLatestByPaTypeIdAsync(validatedForm.PaTypeId);
        if (template == null)
        {
            return ServiceError.NotFound($"Template for PaType {validatedForm.PaTypeId} not found");
        }

        await unitOfWork.Forms.UpdateFormRowValuesAsync(
            request.FormId,
            request.RowOrder,
            filteredValues,
            userId);

        var updatedIndicatorIds = filteredValues.Select(v => v.IndicatorId).ToList();
        var formulaValuesToUpdate = await formRowFormulaCalculator.CalculateFormulaValuesAsync(
            validatedRow,
            template,
            updatedIndicatorIds);

        if (formulaValuesToUpdate.Count != 0)
        {
            await unitOfWork.Forms.UpdateFormRowValuesAsync(
                request.FormId,
                request.RowOrder,
                formulaValuesToUpdate,
                userId);
        }

        await unitOfWork.SaveChangesAsync();

        var updatedForm = await unitOfWork.Forms.FindAsync(request.FormId);
        var updatedRow = updatedForm?.Rows.SingleOrDefault(r => r.Order == request.RowOrder);

        if (updatedRow == null)
        {
            return ServiceError.NotFound($"Form row with Order={request.RowOrder} not found after update");
        }

        return updatedRow.ToRowDto();
    }
}