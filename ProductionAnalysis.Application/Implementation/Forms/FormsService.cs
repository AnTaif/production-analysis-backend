using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Forms;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

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
    IFormValidator formValidator,
    IFormRowUpdateOrchestrator formRowUpdateOrchestrator,
    IFormTotalsUpdater formTotalsUpdater
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
        var validationResult = await formValidator.ValidateCreateRequestAsync(request, creatorId);
        if (!validationResult.IsSuccess)
        {
            return validationResult.Error;
        }

        var (template, employee, shift) = validationResult.Value;
        var context = request.ExtractDomainContext();

        var newForm = new Form(
            0,
            request.PaTypeId,
            FormStatus.InProgress,
            DateTime.UtcNow,
            DateTime.UtcNow,
            context,
            template,
            new List<FormRow>(),
            creatorId,
            shift.Id,
            employee.DepartmentId
        );

        var form = await unitOfWork.Forms.CreateAsync(newForm);

        var schedules = await unitOfWork.Dictionaries.SelectShiftSchedulesByShiftIdAsync(shift.Id);
        var rows = await formRowInitializer.InitializeRowsForShiftAsync(
            shift.StartTime,
            schedules,
            template,
            form.Context);

        unitOfWork.FormRows.AddRows(form.Id, rows);

        await unitOfWork.SaveChangesAsync();

        var createdForm = (await unitOfWork.Forms.FindAsync(form.Id))!;
        await formTotalsUpdater.UpdateTotalsIfNeededAsync(createdForm, createdForm.CreatorId);
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

    public async Task<Result<FormRowDto>> UpdateFormRowAsync(
        int formId,
        short rowOrder,
        UpdateFormRowRequest request,
        Guid userId)
    {
        return await formRowUpdateOrchestrator.UpdateRowAsync(formId, rowOrder, request, userId);
    }
}