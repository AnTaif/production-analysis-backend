namespace ProductionAnalysis.Application.Domain.Forms.Context;

/// <summary>
/// Контекст операций или продуктов для форм типа "Менее 1 шт. в час/смену"
/// Может содержать либо OperationId, либо ProductId
/// </summary>
public class OperationOrProductContext : FormContext
{
    public OperationOrProductContext(int? operationId, int? productId)
    {
        if (operationId.HasValue && productId.HasValue)
            throw new ArgumentException("OperationId and ProductId cannot both be set");
        if (!operationId.HasValue && !productId.HasValue)
            throw new ArgumentException("Either OperationId or ProductId must be set");

        OperationId = operationId;
        ProductId = productId;
    }

    public int? OperationId { get; }
    public int? ProductId { get; }

    public bool IsOperationBased => OperationId.HasValue;
    public bool IsProductBased => ProductId.HasValue;
}