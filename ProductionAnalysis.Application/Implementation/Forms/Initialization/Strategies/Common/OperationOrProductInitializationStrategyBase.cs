using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class OperationOrProductInitializationStrategyBase(
    IOperationService operationService,
    ICleanupOperationHandler cleanupHandler
) : RowInitializationStrategyBase(cleanupHandler)
{
    protected readonly IOperationService OperationService = operationService;

    protected async Task<ICollection<OperationDto>> GetRelatedOperationsAsync(
        OperationOrProductContext operationContext)
    {
        ICollection<OperationDto> relatedOperations;

        if (operationContext is { IsOperationBased: true, OperationId: not null })
        {
            relatedOperations = await OperationService.GetRelatedOperationsAsync(operationContext.OperationId.Value);
        }
        else if (operationContext is { IsProductBased: true, ProductId: not null })
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