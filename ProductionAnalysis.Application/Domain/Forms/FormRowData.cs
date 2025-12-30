namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRowData
{
    public short Order { get; set; }
    public bool IsAdditionalOperation { get; set; }
    public int? AdditionalOperationId { get; set; }
    public Dictionary<string, object> Values { get; set; } = new();
}