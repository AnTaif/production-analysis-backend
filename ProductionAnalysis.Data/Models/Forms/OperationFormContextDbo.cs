using System.Text.Json.Serialization;

namespace ProductionAnalysis.Data.Models.Forms;

/// <summary>
/// DTO для сериализации контекста операций в БД
/// Используется только в Data слое для сериализации/десериализации
/// </summary>
public class OperationFormContextDbo(int operationId) : FormContextBaseDbo
{
    [JsonPropertyName("operationId")]
    public int OperationId { get; } = operationId;
}