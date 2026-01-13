using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms;

/// <summary>
/// DTO для запроса создания формы с контекстом операции или продукта (без названий)
/// </summary>
public record OperationOrProductContextRequest
{
    [Range(1, int.MaxValue)]
    public int? OperationId { get; init; }

    [Range(1, int.MaxValue)]
    public int? ProductId { get; init; }
}