namespace ProductionAnalysis.Application.Domain.Forms;

public class FormRowValue
{
    public FormRowValue(object value)
    {
        Value = value;
    }

    public object Value { get; }
}