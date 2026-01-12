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
        bool hasSummation,
        int order)
    {
        Id = id;
        Name = name;
        ValueType = valueType;
        InputType = inputType;
        ValueSelector = valueSelector;
        Formula = formula;
        HasSummation = hasSummation;
        Order = order;
    }

    public int Id { get; }
    public string Name { get; }
    public string ValueType { get; }
    public string InputType { get; }
    public string? ValueSelector { get; }
    public string? Formula { get; }
    public bool HasSummation { get; }
    public int Order { get; }
}