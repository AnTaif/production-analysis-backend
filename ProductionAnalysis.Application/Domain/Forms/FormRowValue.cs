namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRowValue
{
    public object Value { get; set; } = null!;
    public object? CumulativeValue { get; set; }
}