using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Client.Models.Forms.FormsCreation;

/// <summary>
/// Контекст создания формы для одного продукта.
/// Содержит параметры производства, используемые при расчете формул в таблице формы.
/// </summary>
/// <remarks>
/// Параметры контекста могут использоваться в формулах через ссылки:
/// <list type="bullet">
/// <item><c>context.product.cycleTime</c> - время такта в секундах</item>
/// <item><c>context.product.workstationCapacity</c> - емкость рабочего места</item>
/// <item><c>context.product.dailyRate</c> - дневная норма производства</item>
/// </list>
/// Пример формулы с использованием контекста:
/// <code>
/// (timeToMinutes(indicator_16) / context.product.cycleTime) * context.product.workstationCapacity
/// </code>
/// Где <c>indicator_16</c> - индикатор времени работы в формате "HH:mm-HH:mm"
/// </remarks>
public class CreateFormRequestProductContext : CreateFormRequestContextBase
{
    /// <summary>
    /// Идентификатор продукта
    /// </summary>
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    /// <summary>
    /// Время такта производства в секундах.
    /// Используется для расчета планового количества деталей за промежуток времени.
    /// </summary>
    /// <example>120</example>
    [Range(1, int.MaxValue)]
    public int? CycleTime { get; init; }

    /// <summary>
    /// Емкость рабочего места (количество единиц продукции, которое может быть произведено одновременно).
    /// </summary>
    /// <example>2</example>
    [Range(1, int.MaxValue)]
    public int? WorkstationCapacity { get; init; }

    /// <summary>
    /// Дневная норма производства (количество единиц продукции в день).
    /// </summary>
    /// <example>100</example>
    [Range(1, int.MaxValue)]
    public int DailyRate { get; init; }
}