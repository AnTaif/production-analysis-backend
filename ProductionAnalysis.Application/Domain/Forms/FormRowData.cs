namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRowData
{
    public short Order { get; set; }
    public bool IsAdditionalOperation { get; set; }
    public int? AdditionalOperationId { get; set; }
    public int? ProductId { get; set; }
    public ICollection<FormRowValueData> Values { get; set; } = new List<FormRowValueData>();
}