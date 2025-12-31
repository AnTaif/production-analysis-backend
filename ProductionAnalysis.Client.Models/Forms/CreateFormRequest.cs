using System.ComponentModel.DataAnnotations;
using ProductionAnalysis.Client.Models.Forms.FormsCreation;

namespace ProductionAnalysis.Client.Models.Forms;

/// <summary>
/// Запрос на создание новой формы
/// </summary>
public record CreateFormRequest
{
    /// <summary>
    /// Идентификатор типа производственного анализа
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PaTypeId { get; init; }

    /// <summary>
    /// Идентификатор смены
    /// </summary>
    [Range(1, int.MaxValue)]
    public int ShiftId { get; init; }

    /// <summary>
    /// Словарь контекстов формы.
    /// Ключ - имя контекста (например, "product", "operation"), значение - объект контекста.
    /// </summary>
    /// <remarks>
    /// Контекст содержит дополнительную информацию для расчета формул в таблице формы.
    /// Доступные типы контекста:
    /// <list type="bullet">
    /// <item><see cref="CreateFormRequestProductContext"/> - контекст продукта с параметрами производства (CycleTime, WorkstationCapacity, DailyRate)</item>
    /// <item><see cref="CreateFormRequestProductListContext"/> - контекст списка продуктов</item>
    /// <item><see cref="CreateFormRequestOperationContext"/> - контекст операции</item>
    /// </list>
    /// Пример использования:
    /// <code>
    /// {
    ///   "product": {
    ///     "productId": 1,
    ///     "cycleTime": 120,
    ///     "workstationCapacity": 2,
    ///     "dailyRate": 100
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public required Dictionary<string, CreateFormRequestContextBase> Context { get; init; }
}