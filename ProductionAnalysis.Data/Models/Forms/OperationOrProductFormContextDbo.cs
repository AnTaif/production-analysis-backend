using System.Text.Json.Serialization;

namespace ProductionAnalysis.Data.Models.Forms;

/// <summary>
/// DTO для сериализации контекста операций или продуктов в БД
/// Используется только в Data слое для сериализации/десериализации
/// </summary>
public class OperationOrProductFormContextDbo : FormContextBaseDbo
{
    [JsonPropertyName("operationId")]
    public int? OperationId { get; set; }

    [JsonPropertyName("productId")]
    public int? ProductId { get; set; }
}