using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class OperationOrProductInitializationStrategyBase(IOperationService operationService)
    : RowInitializationStrategyBase
{
    protected readonly IOperationService OperationService = operationService;

    /// <summary>
    ///     Получает связанные операции для контекста операции или продукта
    /// </summary>
    protected async Task<ICollection<OperationDto>> GetRelatedOperationsAsync(
        OperationOrProductContext operationContext)
    {
        ICollection<OperationDto> relatedOperations;

        if (operationContext.IsOperationBased && operationContext.OperationId.HasValue)
        {
            relatedOperations = await OperationService.GetRelatedOperationsAsync(operationContext.OperationId.Value);
        }
        else if (operationContext.IsProductBased && operationContext.ProductId.HasValue)
        {
            relatedOperations =
                await OperationService.GetRelatedOperationsByProductIdAsync(operationContext.ProductId.Value);
        }
        else
        {
            throw new InvalidOperationException(
                "OperationOrProductContext must have either OperationId or ProductId set");
        }

        return relatedOperations.Count != 0
            ? relatedOperations
            : throw new InvalidOperationException(
                operationContext.IsOperationBased
                    ? $"No operations found for operation id {operationContext.OperationId}"
                    : $"No operations found for product id {operationContext.ProductId}");
    }
}