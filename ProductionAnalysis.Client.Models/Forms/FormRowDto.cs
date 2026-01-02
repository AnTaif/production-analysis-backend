namespace ProductionAnalysis.Client.Models.Forms;

public record FormRowDto
{
    public short Order { get; init; }
    public bool IsAdditionalOperation { get; init; }
    public Dictionary<string, FormRowValueDto> Values { get; init; } = new();
}