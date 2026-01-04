using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Domain.Forms;

/// <summary>
/// Расширения для конвертации контекста продукта в DTO
/// </summary>
public static class ProductContextDtoExtensions
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

    /// <summary>
    /// Конвертирует ProductInfo в ProductContextDto
    /// </summary>
    public static ProductContextDto ToDto(this ProductInfo productInfo)
    {
        return new ProductContextDto
        {
            ProductId = productInfo.ProductId,
            CycleTime = productInfo.CycleTime,
            WorkstationCapacity = productInfo.WorkstationCapacity,
            DailyRate = productInfo.DailyRate
        };
    }
}