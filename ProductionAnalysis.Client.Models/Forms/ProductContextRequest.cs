namespace ProductionAnalysis.Client.Models.Forms;

/// <summary>
/// DTO для запроса создания формы с контекстом продукта (без названия)
/// </summary>
public record ProductContextRequest
{
    public int ProductId { get; init; }
    public int? CycleTime { get; init; }
    public int? WorkstationCapacity { get; init; }
    public int DailyRate { get; init; }
}