using Core.Results;
using FluentValidation;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormValidator
{
    Task<Result<(Template template, EmployeeDto employee, EmployeeDto executor, ShiftDto shift)>>
        ValidateCreateRequestAsync(
            CreateFormRequest request,
            Guid creatorId);
}

[RegisterScoped]
public class FormValidator(
    IPaUnitOfWork unitOfWork,
    IValidator<CreateFormRequest> requestValidator
)
    : IFormValidator
{
    public async Task<Result<(Template template, EmployeeDto employee, EmployeeDto executor, ShiftDto shift)>>
        ValidateCreateRequestAsync(
            CreateFormRequest request,
            Guid creatorId)
    {
        // Валидация запроса с помощью FluentValidation
        var validationResult = await requestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors
                .Select(e => string.IsNullOrEmpty(e.PropertyName)
                    ? e.ErrorMessage
                    : $"{e.PropertyName}: {e.ErrorMessage}")
                .ToList();
            return ServiceError.BadRequest(string.Join("; ", errorMessages));
        }

        // Проверка существования зависимостей
        var paTypeId = (int)request.PaType;
        var template = await unitOfWork.Templates.FindLatestVerAsync(paTypeId);
        if (template is null)
        {
            return ServiceError.NotFound($"Template for PaType {request.PaType} not found");
        }

        var creator = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(creatorId);
        if (creator is null)
        {
            return ServiceError.NotFound($"Employee for user {creatorId} not found");
        }

        var executor = await unitOfWork.Dictionaries.FindEmployeeByIdAsync(request.ExecutorId);
        if (executor is null)
        {
            return ServiceError.NotFound($"Executor with id {request.ExecutorId} not found");
        }

        if (executor.DepartmentId != creator.DepartmentId)
        {
            return ServiceError.BadRequest(
                $"Executor and creator must be from the same department. Executor department: {executor.DepartmentId}, Creator department: {creator.DepartmentId}");
        }

        var shift = await unitOfWork.Dictionaries.SelectShiftByIdAsync(request.ShiftId);
        if (shift == null)
        {
            return ServiceError.NotFound($"Shift not found by id {request.ShiftId}");
        }

        return (template, creator, executor, shift);
    }
}