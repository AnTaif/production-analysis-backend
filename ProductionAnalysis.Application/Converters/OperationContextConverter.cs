using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Converters;

/// <summary>
/// Расширения для конвертации контекста операций в DTO
/// </summary>
public static class OperationContextConverter
{
    /// <summary>
    /// Конвертирует OperationContext в OperationContextDto
    /// </summary>
    public static OperationContextDto ToDto(this OperationContext context)
    {
        return new OperationContextDto
        {
            OperationId = context.OperationId
        };
    }
}