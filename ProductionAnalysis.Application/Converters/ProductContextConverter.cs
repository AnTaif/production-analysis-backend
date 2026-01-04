using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

/// <summary>
/// Расширения для конвертации контекста продукта в DTO
/// </summary>
public static class ProductContextConverter
{
    /// <summary>
    /// Конвертирует ProductContext в ProductContextDto
    /// </summary>
    public static ProductContextDto ToDto(this ProductContext context)
    {
        return new ProductContextDto
        {
            ProductId = context.ProductId,
            CycleTime = context.CycleTime,
            WorkstationCapacity = context.WorkstationCapacity,
            DailyRate = context.DailyRate
        };
    }
}