namespace ProductionAnalysis.Application.Domain.Templates;

public class Indicator
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public string? ValueSelector { get; set; }
    public string? Formula { get; set; }
    public bool IsCumulative { get; set; }
    public bool HasSummation { get; set; }
}