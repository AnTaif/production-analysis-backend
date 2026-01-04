using System.Text.Json.Serialization;

namespace ProductionAnalysis.Data.Models.Forms;

/// <summary>
/// DTO для сериализации контекста одного продукта в БД
/// Используется только в Data слое для сериализации/десериализации
/// </summary>
public class ProductFormContextDbo(
    int productId,
    int? cycleTime,
    int? workstationCapacity,
    int dailyRate)
    : FormContextBaseDbo
{
    [JsonPropertyName("productId")]
    public int ProductId { get; } = productId;

    [JsonPropertyName("cycleTime")]
    public int? CycleTime { get; } = cycleTime;

    [JsonPropertyName("workstationCapacity")]
    public int? WorkstationCapacity { get; } = workstationCapacity;

    [JsonPropertyName("dailyRate")]
    public int DailyRate { get; } = dailyRate;
}