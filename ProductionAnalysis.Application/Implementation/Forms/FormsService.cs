using Core.Auth;
using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Forms;
using Shared.Constants;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormsService
{
    Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter, ContextUser user);
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
    IFormTotalsUpdater formTotalsUpdater,
    IFormContextFactory formContextFactory
)
    : IFormsService
{
    public async Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter,
        ContextUser user)
    {
        var domainFilter = searchFilter.ToDomain();

        if (user.Roles.Contains(Roles.Admin))
        {
            // Админ видит все формы, фильтры из запроса применяются как есть
        }
        else if (user.Roles.Contains(Roles.DepartmentHead))
        {
            var employee = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(user.Id);
            if (employee != null)
            {
                domainFilter.DepartmentId = searchFilter.DepartmentId ?? employee.DepartmentId;
            }
            else
            {
                // Если Employee не найден, возвращаем пустой результат
                return new PaginatedResponse<FormShortDto>(
                    [],
                    0,
                    domainFilter.PageNumber,
                    domainFilter.PageSize
                );
            }
        }
        else if (user.Roles.Contains(Roles.Operator))
        {
            var employee = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(user.Id);
            if (employee != null)
            {
                domainFilter.ExecutorId = employee.Id;
            }
            else
            {
                return new PaginatedResponse<FormShortDto>(
                    [],
                    0,
                    domainFilter.PageNumber,
                    domainFilter.PageSize
                );
            }
        }
        else
        {
            return new PaginatedResponse<FormShortDto>(
                [],
                0,
                domainFilter.PageNumber,
                domainFilter.PageSize
            );
        }

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
        if (!validationResult.IsSuccess) return validationResult.Error;

        var (template, employee, executor, shift) = validationResult.Value;
        var context = formContextFactory.CreateContext(request);

        var newForm = new Form(
            0,
            request.PaType.ToDomain(),
            FormStatus.InProgress,
            DateTime.UtcNow,
            DateTime.UtcNow,
            context,
            template,
            new List<FormRow>(),
            creatorId,
            shift.Id,
            employee.DepartmentId,
            executor.Id
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

        if (form == null) return ServiceError.NotFound($"Form with id {formId} not found");

        return form.ToDto();
    }

    public async Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null) return ServiceError.NotFound($"Form with id {formId} not found");

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