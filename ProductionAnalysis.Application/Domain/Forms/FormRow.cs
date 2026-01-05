namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRow
{
    public FormRow(
        short order,
        bool isAuxiliaryOperation,
        int? auxiliaryOperationId,
        Dictionary<string, FormRowValue> values,
        int? productId = null,
        int? groupKey = null)
    {
        Order = order;
        IsAuxiliaryOperation = isAuxiliaryOperation;
        AuxiliaryOperationId = auxiliaryOperationId;
        Values = values;
        ProductId = productId;
        GroupKey = groupKey;
    }

    public short Order { get; }
    public bool IsAuxiliaryOperation { get; }
    public int? AuxiliaryOperationId { get; }
    public Dictionary<string, FormRowValue> Values { get; }
    public int? ProductId { get; }
    public int? GroupKey { get; }
}