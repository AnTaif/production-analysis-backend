namespace ProductionAnalysis.Data.Models.Forms;

public class FormRowDbo
{
    public int FormId { get; set; }

    public short Order { get; set; }

    public bool IsAdditionalOperation { get; set; }

    public int? AdditionalOperationId { get; set; }

    public int? ProductId { get; set; }

    public FormDbo Form { get; set; } = null!;

    public ICollection<FormRowValueDbo> Values { get; set; } = new List<FormRowValueDbo>();
}