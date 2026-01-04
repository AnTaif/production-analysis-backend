using Core.Results;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormValidator
{
    Task<Result<(Template template, EmployeeDto employee, ShiftDto shift)>> ValidateCreateRequestAsync(
        CreateFormRequest request,
        Guid creatorId);
}

[RegisterScoped]
public class FormValidator(IPaUnitOfWork unitOfWork) : IFormValidator
{
    public async Task<Result<(Template template, EmployeeDto employee, ShiftDto shift)>> ValidateCreateRequestAsync(
        CreateFormRequest request,
        Guid creatorId)
    {
        var template = await unitOfWork.Templates.FindLatestVerAsync(request.PaTypeId);
        if (template is null)
        {
            return ServiceError.NotFound($"Template for PaType {request.PaTypeId} not found");
        }

        var employee = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(creatorId);
        if (employee is null)
        {
            return ServiceError.NotFound($"Employee for user {creatorId} not found");
        }

        var shift = await unitOfWork.Dictionaries.SelectShiftByIdAsync(request.ShiftId);
        if (shift == null)
        {
            return ServiceError.NotFound($"Shift not found by id {request.ShiftId}");
        }

        // Валидация ProductContext: должно быть указано либо CycleTime, либо WorkstationCapacity
        if (request.Product != null)
        {
            var hasCycleTime = request.Product.CycleTime.HasValue && request.Product.CycleTime.Value > 0;
            var hasWorkstationCapacity = request.Product.WorkstationCapacity.HasValue &&
                                         request.Product.WorkstationCapacity.Value > 0;

            if (hasCycleTime && hasWorkstationCapacity)
            {
                return ServiceError.BadRequest(
                    "Cannot specify both CycleTime and WorkstationCapacity. Please specify only one of them.");
            }

            if (!hasCycleTime && !hasWorkstationCapacity)
            {
                return ServiceError.BadRequest(
                    "Either CycleTime or WorkstationCapacity must be specified.");
            }
        }

        return (template, employee, shift);
    }
}