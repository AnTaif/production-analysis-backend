using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain;
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
    Task<Result<FormRowDto>> UpdateFormRowAsync(UpdateFormRowRequest request, Guid userId);
}

[RegisterScoped]
public class FormsService(
    IPaUnitOfWork unitOfWork,
    IFormRowGenerator formRowGenerator
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
        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {request.FormId} not found");
        }

        var row = form.Rows.SingleOrDefault(r => r.Order == request.RowOrder);
        if (row == null)
        {
            return ServiceError.NotFound($"Form row with Order={request.RowOrder} not found in form {request.FormId}");
        }

        var templateSnapshot = FormTemplateParser.ParseTemplateSnapshot(form.TemplateSnapshot);
        var indicatorsDict = templateSnapshot.TableColumns
            .Where(c => c.Id > 0 && !string.IsNullOrEmpty(c.InputType))
            .ToDictionary(c => c.Id, c => c.InputType);

        var filteredValues = new List<FormRowValueData>();
        foreach (var (indicatorId, value) in request.Values)
        {
            if (!indicatorsDict.TryGetValue(indicatorId, out var inputType))
            {
                continue;
            }

            if (inputType is FieldInputTypes.Manual or FieldInputTypes.Dictionary)
            {
                filteredValues.Add(new FormRowValueData
                {
                    IndicatorId = indicatorId,
                    Value = value
                });
            }
        }

        await unitOfWork.Forms.UpdateFormRowValuesAsync(
            request.FormId,
            request.RowOrder,
            filteredValues,
            userId);

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