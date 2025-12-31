namespace ProductionAnalysis.Client.Models.Forms.FormsCreation;

/// <summary>
/// Контекст создания формы для списка продуктов.
/// Используется, когда форма содержит несколько продуктов с разными параметрами производства.
/// </summary>
/// <remarks>
/// Каждый продукт в списке имеет свои параметры (CycleTime, WorkstationCapacity, DailyRate),
/// которые могут использоваться в формулах при расчете плановых значений.
/// </remarks>
public class CreateFormRequestProductListContext : CreateFormRequestContextBase
{
    /// <summary>
    /// Список продуктов с их параметрами производства
    /// </summary>
    public required ICollection<CreateFormRequestProductContext> Products { get; set; }
}