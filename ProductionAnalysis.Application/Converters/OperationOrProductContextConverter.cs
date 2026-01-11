using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

/// <summary>
/// Расширения для конвертации контекста операций или продуктов в DTO
/// </summary>
public static class OperationOrProductContextConverter
{
    /// <summary>
    /// Конвертирует OperationOrProductContext в OperationOrProductContextDto
    /// </summary>
    public static OperationOrProductContextDto ToDto(this OperationOrProductContext context)
    {
        return new OperationOrProductContextDto
        {
            OperationId = context.OperationId,
            ProductId = context.ProductId
        };
    }
}