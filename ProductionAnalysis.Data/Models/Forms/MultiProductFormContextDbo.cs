using System.Text.Json.Serialization;

namespace ProductionAnalysis.Data.Models.Forms;

/// <summary>
/// DTO для сериализации контекста нескольких продуктов в БД
/// Используется только в Data слое для сериализации/десериализации
/// </summary>
public class MultiProductFormContextDbo(ICollection<ProductFormContextDbo> products) : FormContextBaseDbo
{
    [JsonPropertyName("products")]
    public ICollection<ProductFormContextDbo> Products { get; } = products;
}