namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRowValue
{
    public FormRowValue(object value, object? cumulativeValue)
    {
        Value = value;
        CumulativeValue = cumulativeValue;
    }

    public object Value { get; }
    public object? CumulativeValue { get; }
}