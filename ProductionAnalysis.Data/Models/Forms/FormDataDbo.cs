using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Models.Forms;

public class FormDataDbo
{
    public int Id { get; set; }

    public int FormId { get; set; }

    public int IndicatorId { get; set; }

    public short RowOrder { get; set; }

    public required string Value { get; set; }

    public bool IsCalculated { get; set; }

    public DateTime? CalculatedAt { get; set; }

    public DateTime LastModifiedAt { get; set; }

    public Guid LastModifiedBy { get; set; }

    public FormDbo Form { get; set; } = null!;

    public IndicatorDbo Indicator { get; set; } = null!;
}