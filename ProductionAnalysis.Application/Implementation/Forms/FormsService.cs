using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Forms;
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
    Task<Result<FormRowDto>> UpdateFormRowAsync(int formId, short rowOrder, UpdateFormRowRequest request, Guid userId);
}

[RegisterScoped]
public class FormsService(
    IPaUnitOfWork unitOfWork,
    IFormRowInitializer formRowInitializer,
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
        var template = await unitOfWork.Templates.FindLatestVerAsync(request.PaTypeId);
        if (template == null)
        {
            return ServiceError.NotFound($"Template for PaType {request.PaTypeId} not found");
        }

        var employee = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(creatorId);
        if (employee == null)
        {
            return ServiceError.NotFound($"Employee for user {creatorId} not found");
        }

        var context = request.Context.ToDomainContext();

        var newForm = new Form
        {
            PaTypeId = request.PaTypeId,
            TemplateSnapshot = TemplateSerializer.SerializeTemplateSnapshot(template),
            Context = context,
            CreatorId = creatorId,
            ShiftId = request.ShiftId,
            DepartmentId = employee.DepartmentId
        };

        var form = await unitOfWork.Forms.CreateAsync(newForm);

        await CreateFormRowsIfNeededAsync(form.Id, request.ShiftId, template);
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
        var rows = await formRowInitializer.InitializeRowsForShiftAsync(
            shift.StartTime,
            schedules,
            template);

        await unitOfWork.Forms.CreateFormRowsAsync(formId, rows);
    }

    public async Task<Result<FormRowDto>> UpdateFormRowAsync(int formId, short rowOrder, UpdateFormRowRequest request,
        Guid userId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form is null)
        {
            return ServiceError.NotFound($"Form {formId} not found");
        }

        var row = form.Rows.SingleOrDefault(r => r.Order == rowOrder);
        if (row is null)
        {
            return ServiceError.NotFound($"Row {rowOrder} not found in form {formId}");
        }

        var filteredValues = formRowValueFilter.FilterUpdatableValues(
            request.Values,
            form.TemplateSnapshot);

        if (filteredValues.Count == 0)
        {
            return row.ToRowDto();
        }

        var template = await unitOfWork.Templates.FindLatestVerAsync(form.PaTypeId);
        if (template == null)
        {
            return ServiceError.NotFound($"Template for PaType {form.PaTypeId} not found");
        }

        await unitOfWork.Forms.UpdateFormRowValuesAsync(
            formId,
            rowOrder,
            filteredValues,
            userId);

        var updatedIndicatorIds = filteredValues.Select(v => v.IndicatorId).ToList();
        var formulaValuesToUpdate = await formRowFormulaCalculator.CalculateFormulaValuesAsync(
            row,
            template,
            updatedIndicatorIds,
            form.Context);

        if (formulaValuesToUpdate.Count != 0)
        {
            await unitOfWork.Forms.UpdateFormRowValuesAsync(
                formId,
                rowOrder,
                formulaValuesToUpdate,
                userId);
        }

        await unitOfWork.SaveChangesAsync();

        var updatedForm = await unitOfWork.Forms.FindAsync(formId);
        var updatedRow = updatedForm?.Rows.SingleOrDefault(r => r.Order == rowOrder);

        if (updatedRow == null)
        {
            return ServiceError.NotFound($"Form row with Order={rowOrder} not found after update");
        }

        return updatedRow.ToRowDto();
    }
}