using System.Text.Json.Serialization;

namespace ProductionAnalysis.Data.Models.Forms;

/// <summary>
/// DTO для сериализации контекста нескольких продуктов в БД
/// Используется только в Data слое для сериализации/десериализации
/// </summary>
public class MultiProductFormContextDbo(ICollection<ProductInfoDbo> products) : FormContextBaseDbo
{
    [JsonPropertyName("products")]
    public ICollection<ProductInfoDbo> Products { get; } = products;
}

/// <summary>
/// DTO для сериализации информации о продукте в БД
/// </summary>
public class ProductInfoDbo(
    int productId,
    int? cycleTime,
    int? workstationCapacity,
    int dailyRate)
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