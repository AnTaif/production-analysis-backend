using Core.Results;
using ProductionAnalysis.Application.Domain.Forms;
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
        var paTypeId = (int)request.PaType;
        var template = await unitOfWork.Templates.FindLatestVerAsync(paTypeId);
        if (template is null)
        {
            return ServiceError.NotFound($"Template for PaType {request.PaType} not found");
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

        var paType = ConvertToDomainPaType(request.PaType);
        var validationResult = paType switch
        {
            PaType.SingleProductWithCycleTime => ValidateSingleProductWithCycleTime(request),
            PaType.SingleProductWithWorkstationCapacity => ValidateSingleProductWithWorkstationCapacity(request),
            PaType.MultipleProductsWithCycleTime => ValidateMultipleProductsWithCycleTime(request),
            PaType.LessThanOnePerHour => ValidateLessThanOnePerHour(request),
            _ => throw new NotSupportedException($"Unknown form type: {request.PaType}")
        };

        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        return (template, employee, shift);
    }

    private static Result ValidateSingleProductWithCycleTime(CreateFormRequest request)
    {
        if (request.Product == null)
        {
            return ServiceError.BadRequest(
                $"Product is required for {PaType.SingleProductWithCycleTime}");
        }

        if (request.Product.CycleTime is not > 0)
        {
            return ServiceError.BadRequest(
                $"CycleTime is required and must be greater than 0 for {PaType.SingleProductWithCycleTime}");
        }

        return Result.Success;
    }

    private static Result ValidateSingleProductWithWorkstationCapacity(CreateFormRequest request)
    {
        if (request.Product == null)
        {
            return ServiceError.BadRequest(
                $"Product is required for {PaType.SingleProductWithWorkstationCapacity}");
        }

        if (request.Product.WorkstationCapacity is not > 0)
        {
            return ServiceError.BadRequest(
                $"WorkstationCapacity is required and must be greater than 0 for {PaType.SingleProductWithWorkstationCapacity}");
        }

        return Result.Success;
    }

    private static Result ValidateMultipleProductsWithCycleTime(CreateFormRequest request)
    {
        if (request.Products == null || request.Products.Count == 0)
        {
            return ServiceError.BadRequest(
                $"Products are required for {PaType.MultipleProductsWithCycleTime}");
        }

        if (request.Products.Any(product => product.CycleTime is not > 0))
        {
            return ServiceError.BadRequest(
                $"CycleTime is required and must be greater than 0 for all products in {PaType.MultipleProductsWithCycleTime}");
        }

        return Result.Success;
    }

    private static Result ValidateLessThanOnePerHour(CreateFormRequest request)
    {
        if (request.Operation == null)
        {
            return ServiceError.BadRequest(
                $"Operation is required for {PaType.LessThanOnePerHour}");
        }

        if (request.Operation.OperationId <= 0)
        {
            return ServiceError.BadRequest(
                $"OperationId must be greater than 0 for {PaType.LessThanOnePerHour}");
        }

        return Result.Success;
    }

    private static PaType ConvertToDomainPaType(PaTypeDto paTypeDto)
    {
        return paTypeDto switch
        {
            PaTypeDto.SingleProductWithCycleTime => PaType.SingleProductWithCycleTime,
            PaTypeDto.SingleProductWithWorkstationCapacity => PaType.SingleProductWithWorkstationCapacity,
            PaTypeDto.MultipleProductsWithCycleTime => PaType.MultipleProductsWithCycleTime,
            PaTypeDto.LessThanOnePerHour => PaType.LessThanOnePerHour,
            _ => throw new ArgumentOutOfRangeException(nameof(paTypeDto), paTypeDto, "Unknown PaTypeDto value")
        };
    }
}