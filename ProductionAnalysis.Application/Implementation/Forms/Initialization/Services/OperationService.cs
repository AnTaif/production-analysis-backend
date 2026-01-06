using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IOperationService
{
    Task<ICollection<OperationDto>> LoadOperationsAsync();
    Task<ICollection<OperationDto>> GetRelatedOperationsAsync(int operationId);
    double CalculateCycleDuration(ICollection<OperationDto> operations);
}

[RegisterScoped]
public class OperationService(IPaUnitOfWork unitOfWork) : IOperationService
{
    public async Task<ICollection<OperationDto>> LoadOperationsAsync()
    {
        return await unitOfWork.Dictionaries.SelectOperationsAsync();
    }

    public async Task<ICollection<OperationDto>> GetRelatedOperationsAsync(int operationId)
    {
        var allOperations = await LoadOperationsAsync();
        var operationsById = allOperations.ToDictionary(op => op.Id);
        var result = new List<OperationDto>();
        var visited = new HashSet<int>();

        if (!operationsById.TryGetValue(operationId, out var mainOperation)) return result;

        // Добавляем основную операцию
        result.Add(mainOperation);
        visited.Add(operationId);

        // Собираем все операции, которые связаны с основной операцией или продуктом
        // через BasedOperationId или BasedProductId
        CollectRelatedOperations(mainOperation, operationsById, result, visited);

        return result;
    }

    public double CalculateCycleDuration(ICollection<OperationDto> operations)
    {
        // Суммируем длительность всех операций в цикле
        return operations
            .Where(op => op.Duration.HasValue)
            .Sum(op => op.Duration.Value.TotalSeconds);
    }

    private static void CollectRelatedOperations(
        OperationDto operation,
        Dictionary<int, OperationDto> operationsById,
        List<OperationDto> result,
        HashSet<int> visited)
    {
        // Если операция основана на другой операции, добавляем эту операцию и её связанные
        if (operation.BasedOnType == OperationBasedOnType.Operation && operation.BasedOperationId.HasValue)
        {
            var basedOperationId = operation.BasedOperationId.Value;
            if (!visited.Contains(basedOperationId) &&
                operationsById.TryGetValue(basedOperationId, out var basedOperation))
            {
                visited.Add(basedOperationId);
                result.Add(basedOperation);
                CollectRelatedOperations(basedOperation, operationsById, result, visited);
            }
        }

        // Если операция связана с продуктом, собираем все операции, связанные с тем же продуктом
        if (operation.BasedOnType == OperationBasedOnType.Product && operation.BasedProductId.HasValue)
        {
            var productRelatedOperations = operationsById.Values
                .Where(op => !visited.Contains(op.Id) &&
                             op.BasedOnType == OperationBasedOnType.Product &&
                             op.BasedProductId == operation.BasedProductId);

            foreach (var productOp in productRelatedOperations)
            {
                visited.Add(productOp.Id);
                result.Add(productOp);
                CollectRelatedOperations(productOp, operationsById, result, visited);
            }
        }

        // Собираем операции, которые основаны на текущей операции
        var dependentOperations = operationsById.Values
            .Where(op => !visited.Contains(op.Id) &&
                         op.BasedOnType == OperationBasedOnType.Operation &&
                         op.BasedOperationId == operation.Id);

        foreach (var dependentOp in dependentOperations)
        {
            visited.Add(dependentOp.Id);
            result.Add(dependentOp);
            CollectRelatedOperations(dependentOp, operationsById, result, visited);
        }
    }
}