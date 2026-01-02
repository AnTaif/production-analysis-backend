using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Models.Forms;

public class FormRowValueDbo
{
    public int FormId { get; set; }

    public short FormRowOrder { get; set; }

    public int IndicatorId { get; set; }

    public required string Value { get; set; }

    public string? CumulativeValue { get; set; }

    public FormRowDbo FormRow { get; set; } = null!;

    public IndicatorDbo Indicator { get; set; } = null!;
}