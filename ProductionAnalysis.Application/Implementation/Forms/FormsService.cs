using Core.Auth;
using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
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

    Task<Result<UpdateFormRowResponse>> UpdateFormRowAsync(int formId, short rowOrder, UpdateFormRowRequest request,
        Guid userId);

    Task<Result> CompleteFormAsync(int formId, Guid userId);

    Task<Result> DeleteFormAsync(int formId, ContextUser user);

    Task<Result<FormCountsDto>> GetFormCountsAsync(ContextUser user);
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
                domainFilter.AssigneeId = employee.Id;
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

        // Собираем все ID продуктов и операций из контекстов форм
        var productIds = new HashSet<int>();
        var operationIds = new HashSet<int>();

        foreach (var form in forms)
        {
            var productContext = form.Context.GetProductContext();
            if (productContext != null)
            {
                productIds.Add(productContext.ProductId);
            }

            var multiProductContext = form.Context.GetMultiProductContext();
            if (multiProductContext != null)
            {
                foreach (var product in multiProductContext.Products)
                {
                    productIds.Add(product.ProductId);
                }
            }

            var operationOrProductContext = form.Context.GetOperationOrProductContext();
            if (operationOrProductContext != null)
            {
                if (operationOrProductContext.OperationId.HasValue)
                {
                    operationIds.Add(operationOrProductContext.OperationId.Value);
                }
                else if (operationOrProductContext.ProductId.HasValue)
                {
                    productIds.Add(operationOrProductContext.ProductId.Value);
                }
            }
        }

        var shiftIds = forms.Select(f => f.ShiftId).Distinct().ToHashSet();

        var allProducts = await unitOfWork.Dictionaries.SelectProductsAsync();
        var allOperations = await unitOfWork.Dictionaries.SelectOperationsAsync();
        var allShifts = await unitOfWork.Dictionaries.SelectShiftsAsync();

        var productsById = allProducts
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id, p => p.Name);

        var operationsById = allOperations
            .Where(o => operationIds.Contains(o.Id))
            .ToDictionary(o => o.Id, o => o.Name);

        var shiftsById = allShifts
            .Where(s => shiftIds.Contains(s.Id))
            .ToDictionary(s => s.Id);

        var dtos = new List<FormShortDto>();
        foreach (var form in forms)
        {
            var creator = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(form.CreatorId);
            var assignee = await unitOfWork.Dictionaries.FindEmployeeByIdAsync(form.AssigneeId);

            if (creator == null || assignee == null)
            {
                // Пропускаем форму, если не удалось загрузить данные о создателе или исполнителе
                continue;
            }

            if (!shiftsById.TryGetValue(form.ShiftId, out var shift))
            {
                // Пропускаем форму, если не удалось загрузить данные о смене
                continue;
            }

            dtos.Add(form.ToShortDto(creator, assignee, shift, productsById, operationsById));
        }

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

        var (template, creator, assignee, shift) = validationResult.Value;
        var context = formContextFactory.CreateContext(request);

        var newForm = new Form(
            0,
            request.PaType.ToDomain(),
            FormStatus.InProgress,
            DateTime.UtcNow,
            DateTime.UtcNow,
            request.FormDate,
            context,
            template,
            new List<FormRow>(),
            creatorId,
            shift.Id,
            creator.DepartmentId,
            assignee.Id
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

        // Загружаем продукты и операции для отображения названий
        var productIds = new HashSet<int>();
        var operationIds = new HashSet<int>();

        var productContext = createdForm.Context.GetProductContext();
        if (productContext != null)
        {
            productIds.Add(productContext.ProductId);
        }

        var multiProductContext = createdForm.Context.GetMultiProductContext();
        if (multiProductContext != null)
        {
            foreach (var product in multiProductContext.Products)
            {
                productIds.Add(product.ProductId);
            }
        }

        var operationOrProductContext = createdForm.Context.GetOperationOrProductContext();
        if (operationOrProductContext != null)
        {
            if (operationOrProductContext.OperationId.HasValue)
            {
                operationIds.Add(operationOrProductContext.OperationId.Value);
            }
            else if (operationOrProductContext.ProductId.HasValue)
            {
                productIds.Add(operationOrProductContext.ProductId.Value);
            }
        }

        var allProducts = await unitOfWork.Dictionaries.SelectProductsAsync();
        var allOperations = await unitOfWork.Dictionaries.SelectOperationsAsync();

        var productsById = allProducts
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id, p => p.Name);

        var operationsById = allOperations
            .Where(o => operationIds.Contains(o.Id))
            .ToDictionary(o => o.Id, o => o.Name);

        return createdForm.ToShortDto(creator, assignee, shift, productsById, operationsById);
    }

    public async Task<Result<FormDto>> GetByIdAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null) return ServiceError.NotFound($"Form with id {formId} not found");

        // Загружаем смену
        var shift = await unitOfWork.Dictionaries.SelectShiftByIdAsync(form.ShiftId);
        if (shift == null)
        {
            return ServiceError.NotFound($"Shift with id {form.ShiftId} not found");
        }

        // Загружаем департамент
        var allDepartments = await unitOfWork.Dictionaries.SelectDepartmentsAsync();
        var department = allDepartments.FirstOrDefault(d => d.Id == form.DepartmentId);
        if (department == null)
        {
            return ServiceError.NotFound($"Department with id {form.DepartmentId} not found");
        }

        // Загружаем создателя
        var creator = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(form.CreatorId);
        if (creator == null)
        {
            return ServiceError.NotFound($"Creator with id {form.CreatorId} not found");
        }

        // Загружаем исполнителя
        var assignee = await unitOfWork.Dictionaries.FindEmployeeByIdAsync(form.AssigneeId);
        if (assignee == null)
        {
            return ServiceError.NotFound($"Assignee with id {form.AssigneeId} not found");
        }

        // Собираем ID продуктов и операций из контекста
        var productIds = new HashSet<int>();
        var operationIds = new HashSet<int>();

        var productContext = form.Context.GetProductContext();
        if (productContext != null)
        {
            productIds.Add(productContext.ProductId);
        }

        var multiProductContext = form.Context.GetMultiProductContext();
        if (multiProductContext != null)
        {
            foreach (var product in multiProductContext.Products)
            {
                productIds.Add(product.ProductId);
            }
        }

        var operationOrProductContext = form.Context.GetOperationOrProductContext();
        if (operationOrProductContext != null)
        {
            if (operationOrProductContext.OperationId.HasValue)
            {
                operationIds.Add(operationOrProductContext.OperationId.Value);
            }
            else if (operationOrProductContext.ProductId.HasValue)
            {
                productIds.Add(operationOrProductContext.ProductId.Value);
            }
        }

        // Загружаем продукты и операции
        var allProducts = await unitOfWork.Dictionaries.SelectProductsAsync();
        var allOperations = await unitOfWork.Dictionaries.SelectOperationsAsync();

        var productsById = allProducts
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id, p => p.Name);

        var operationsById = allOperations
            .Where(o => operationIds.Contains(o.Id))
            .ToDictionary(o => o.Id, o => o.Name);

        return form.ToDto(shift, department, creator, assignee, productsById, operationsById);
    }

    public async Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null) return ServiceError.NotFound($"Form with id {formId} not found");

        return form.Rows.ToRowDtos();
    }

    public async Task<Result<UpdateFormRowResponse>> UpdateFormRowAsync(
        int formId,
        short rowOrder,
        UpdateFormRowRequest request,
        Guid userId)
    {
        return await formRowUpdateOrchestrator.UpdateRowAsync(formId, rowOrder, request, userId);
    }

    public async Task<Result> CompleteFormAsync(int formId, Guid userId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {formId} not found");
        }

        if (form.Status == FormStatus.Completed)
        {
            return ServiceError.Conflict($"Form {formId} is already completed");
        }

        await unitOfWork.Forms.UpdateStatusAsync(formId, FormStatus.Completed, userId);
        await unitOfWork.SaveChangesAsync();

        return Result.Success;
    }

    public async Task<Result> DeleteFormAsync(int formId, ContextUser user)
    {
        var isAdmin = user.Roles.Contains(Roles.Admin);
        var isDepartmentHead = user.Roles.Contains(Roles.DepartmentHead);

        if (!isAdmin && !isDepartmentHead)
        {
            return ServiceError.Forbidden("Only Admin or DepartmentHead can delete forms");
        }

        var form = await unitOfWork.Forms.FindAsync(formId);
        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {formId} not found");
        }

        if (isDepartmentHead && !isAdmin && form.CreatorId != user.Id)
        {
            return ServiceError.Forbidden("DepartmentHead can only delete forms created by themselves");
        }

        await unitOfWork.Forms.DeleteAsync(formId);
        await unitOfWork.SaveChangesAsync();

        return Result.Success;
    }

    public async Task<Result<FormCountsDto>> GetFormCountsAsync(ContextUser user)
    {
        int? departmentId = null;
        int? assigneeId = null;

        if (user.Roles.Contains(Roles.Admin))
        {
            // Админ видит все формы, фильтры не применяются
        }
        else if (user.Roles.Contains(Roles.DepartmentHead))
        {
            var employee = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(user.Id);
            if (employee != null)
            {
                departmentId = employee.DepartmentId;
            }
            else
            {
                // Если Employee не найден, возвращаем нулевые счетчики
                return new FormCountsDto
                {
                    Total = 0,
                    InProgress = 0,
                    Completed = 0
                };
            }
        }
        else if (user.Roles.Contains(Roles.Operator))
        {
            var employee = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(user.Id);
            if (employee != null)
            {
                assigneeId = employee.Id;
            }
            else
            {
                return new FormCountsDto
                {
                    Total = 0,
                    InProgress = 0,
                    Completed = 0
                };
            }
        }
        else
        {
            return new FormCountsDto
            {
                Total = 0,
                InProgress = 0,
                Completed = 0
            };
        }

        var (total, inProgress, completed) = await unitOfWork.Forms.GetFormCountsAsync(departmentId, assigneeId);

        return new FormCountsDto
        {
            Total = total,
            InProgress = inProgress,
            Completed = completed
        };
    }
}