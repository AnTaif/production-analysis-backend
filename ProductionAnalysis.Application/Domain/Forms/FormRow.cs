namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRow
{
    public FormRow(
        short order,
        bool isAdditionalOperation,
        int? additionalOperationId,
        Dictionary<string, FormRowValue> values)
    {
        Order = order;
        IsAdditionalOperation = isAdditionalOperation;
        AdditionalOperationId = additionalOperationId;
        Values = values;
    }

    public short Order { get; }
    public bool IsAdditionalOperation { get; }
    public int? AdditionalOperationId { get; }
    public Dictionary<string, FormRowValue> Values { get; }
}