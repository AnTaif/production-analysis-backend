namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRow
{
    public short Order { get; set; }
    public bool IsAdditionalOperation { get; set; }
    public int? AdditionalOperationId { get; set; }
    public Dictionary<string, FormRowValue> Values { get; set; } = new();
}