namespace ProductionAnalysis.Application.Implementation.Forms;

public sealed class ProductContext
{
    public required int DailyRate { get; init; }
    public int? CycleTime { get; init; }
    public int? WorkstationCapacity { get; init; }
}