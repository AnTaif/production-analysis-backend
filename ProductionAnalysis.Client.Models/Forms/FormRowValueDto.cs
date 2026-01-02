namespace ProductionAnalysis.Client.Models.Forms;

public record FormRowValueDto
{
    public object Value { get; init; } = null!;
    public object? CumulativeValue { get; init; }
}