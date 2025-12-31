namespace ProductionAnalysis.Client.Models.Forms;

public record FormFieldDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string InputType { get; init; } = string.Empty;
    public string? InputSelector { get; init; }
    public string? ValueType { get; init; }
}