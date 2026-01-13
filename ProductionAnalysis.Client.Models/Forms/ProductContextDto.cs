namespace ProductionAnalysis.Client.Models.Forms;

public record ProductContextDto
{
    public int ProductId { get; init; }
    public int? CycleTime { get; init; }
    public int? WorkstationCapacity { get; init; }
    public int DailyRate { get; init; }
    public string ProductName { get; init; } = string.Empty;
}