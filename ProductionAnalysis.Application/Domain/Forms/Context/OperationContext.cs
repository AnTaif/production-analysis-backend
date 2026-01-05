namespace ProductionAnalysis.Application.Domain.Forms.Context;

/// <summary>
/// Контекст операций для форм типа "Менее 1 шт. в час"
/// </summary>
public class OperationContext : FormContext
{
    public OperationContext(int operationId)
    {
        OperationId = operationId;
    }

    public int OperationId { get; }
}