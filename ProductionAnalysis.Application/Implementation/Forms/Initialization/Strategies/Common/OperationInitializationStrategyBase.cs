using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class OperationInitializationStrategyBase(IOperationService operationService)
    : RowInitializationStrategyBase
{
    protected readonly IOperationService OperationService = operationService;

    /// <summary>
    ///     Получает связанные операции для контекста операции
    /// </summary>
    protected async Task<ICollection<OperationDto>> GetRelatedOperationsAsync(OperationContext operationContext)
    {
        var relatedOperations = await OperationService.GetRelatedOperationsAsync(operationContext.OperationId);

        return relatedOperations.Count != 0
            ? relatedOperations
            : throw new InvalidOperationException(
                $"No operations found for operation id {operationContext.OperationId}");
    }
}