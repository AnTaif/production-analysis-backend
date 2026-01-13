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
    public static OperationOrProductContextDto ToDto(
        this OperationOrProductContext context,
        Dictionary<int, string>? productsById = null,
        Dictionary<int, string>? operationsById = null)
    {
        string operationName = string.Empty;
        string productName = string.Empty;

        if (context.OperationId.HasValue)
        {
            operationName = operationsById?.TryGetValue(context.OperationId.Value, out var name) == true
                ? name
                : string.Empty;
        }

        if (context.ProductId.HasValue)
        {
            productName = productsById?.TryGetValue(context.ProductId.Value, out var name) == true
                ? name
                : string.Empty;
        }

        return new OperationOrProductContextDto
        {
            OperationId = context.OperationId,
            ProductId = context.ProductId,
            OperationName = operationName,
            ProductName = productName
        };
    }
}