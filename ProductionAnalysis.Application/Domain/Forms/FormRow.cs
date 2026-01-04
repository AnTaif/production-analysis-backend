namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRow
{
    public FormRow(
        short order,
        bool isAdditionalOperation,
        int? additionalOperationId,
        Dictionary<string, FormRowValue> values,
        int? productId = null)
    {
        Order = order;
        IsAdditionalOperation = isAdditionalOperation;
        AdditionalOperationId = additionalOperationId;
        Values = values;
        ProductId = productId;
    }

    public short Order { get; }
    public bool IsAdditionalOperation { get; }
    public int? AdditionalOperationId { get; }
    public Dictionary<string, FormRowValue> Values { get; }
    public int? ProductId { get; }
}