using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IOperationsService
{
    Task<Result<OperationDto>> CreateOperationAsync(CreateOperationRequest request);
    Task<Result<OperationDto>> UpdateOperationAsync(int operationId, UpdateOperationRequest request);
    Task<Result> DeleteOperationAsync(int operationId);
}

[RegisterScoped]
public class OperationsService(IPaUnitOfWork unitOfWork) : IOperationsService
{
    public async Task<Result<OperationDto>> CreateOperationAsync(CreateOperationRequest request)
    {
        // Валидация зависимостей
        if (request.BasedOnType == OperationBasedOnType.Operation && request.BasedOperationId.HasValue)
        {
            var operationExists = await unitOfWork.Dictionaries.OperationExistsAsync(request.BasedOperationId.Value);
            if (!operationExists)
            {
                return ServiceError.NotFound($"Operation with id {request.BasedOperationId.Value} not found");
            }
        }

        if (request.BasedOnType == OperationBasedOnType.Product && request.BasedProductId.HasValue)
        {
            var productExists = await unitOfWork.Dictionaries.ProductExistsAsync(request.BasedProductId.Value);
            if (!productExists)
            {
                return ServiceError.NotFound($"Product with id {request.BasedProductId.Value} not found");
            }
        }

        var operation = await unitOfWork.Dictionaries.CreateOperationAsync(request);
        return operation;
    }

    public async Task<Result<OperationDto>> UpdateOperationAsync(int operationId, UpdateOperationRequest request)
    {
        var existingOperation = await unitOfWork.Dictionaries.FindOperationByIdAsync(operationId);
        if (existingOperation == null)
        {
            return ServiceError.NotFound($"Operation with id {operationId} not found");
        }

        // Валидация зависимостей
        if (request.BasedOnType == OperationBasedOnType.Operation && request.BasedOperationId.HasValue)
        {
            var operationExists = await unitOfWork.Dictionaries.OperationExistsAsync(request.BasedOperationId.Value);
            if (!operationExists)
            {
                return ServiceError.NotFound($"Operation with id {request.BasedOperationId.Value} not found");
            }
        }

        if (request.BasedOnType == OperationBasedOnType.Product && request.BasedProductId.HasValue)
        {
            var productExists = await unitOfWork.Dictionaries.ProductExistsAsync(request.BasedProductId.Value);
            if (!productExists)
            {
                return ServiceError.NotFound($"Product with id {request.BasedProductId.Value} not found");
            }
        }

        var updatedOperation = await unitOfWork.Dictionaries.UpdateOperationAsync(operationId, request);
        if (updatedOperation == null)
        {
            return ServiceError.NotFound($"Operation with id {operationId} not found");
        }

        return updatedOperation;
    }

    public async Task<Result> DeleteOperationAsync(int operationId)
    {
        var operation = await unitOfWork.Dictionaries.FindOperationByIdAsync(operationId);
        if (operation == null)
        {
            return ServiceError.NotFound($"Operation with id {operationId} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeleteOperationAsync(operationId);
        if (!deleted)
        {
            return ServiceError.NotFound($"Operation with id {operationId} not found");
        }

        return Result.Success;
    }
}