using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms;

/// <summary>
/// Контекст для операций или продуктов в формах типа "Менее 1 шт. в час/смену"
/// Может содержать либо OperationId, либо ProductId
/// </summary>
public record OperationOrProductContextDto
{
    [Range(1, int.MaxValue)]
    public int? OperationId { get; init; }

    [Range(1, int.MaxValue)]
    public int? ProductId { get; init; }
}