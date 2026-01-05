namespace ProductionAnalysis.Client.Models.Forms;

public record FormRowDto
{
    public short Order { get; init; }
    public bool IsAuxiliaryOperation { get; init; }
    public int? ProductId { get; init; }
    public Dictionary<string, FormRowValueDto> Values { get; init; } = new();
}