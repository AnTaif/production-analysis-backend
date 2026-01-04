namespace ProductionAnalysis.Application.Domain.Templates;

public class Indicator
{
    public Indicator(
        int id,
        string name,
        string valueType,
        string inputType,
        string? valueSelector,
        string? formula,
        bool isCumulative,
        bool hasSummation)
    {
        Id = id;
        Name = name;
        ValueType = valueType;
        InputType = inputType;
        ValueSelector = valueSelector;
        Formula = formula;
        IsCumulative = isCumulative;
        HasSummation = hasSummation;
    }

    public int Id { get; }
    public string Name { get; }
    public string ValueType { get; }
    public string InputType { get; }
    public string? ValueSelector { get; }
    public string? Formula { get; }
    public bool IsCumulative { get; }
    public bool HasSummation { get; }
}