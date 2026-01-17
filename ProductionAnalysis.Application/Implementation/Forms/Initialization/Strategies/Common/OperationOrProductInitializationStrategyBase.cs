using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class OperationOrProductInitializationStrategyBase(
    IOperationService operationService,
    ICleanupOperationHandler cleanupHandler,
    IFormRowEndTimeExtractor endTimeExtractor,
    IBreakProcessor breakProcessor
) : RowInitializationStrategyBase(cleanupHandler, endTimeExtractor, breakProcessor)
{
    protected readonly IOperationService OperationService = operationService;

    protected ICollection<OperationDto> GetRelatedOperations(
        OperationOrProductContext operationContext,
        ICollection<OperationDto> allOperations)
    {
        ICollection<OperationDto> relatedOperations;

        if (operationContext is { IsOperationBased: true, OperationId: not null })
        {
            relatedOperations =
                OperationService.GetRelatedOperations(operationContext.OperationId.Value, allOperations);
        }
        else if (operationContext is { IsProductBased: true, ProductId: not null })
        {
            relatedOperations =
                OperationService.GetRelatedOperationsByProductId(operationContext.ProductId.Value, allOperations);
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