using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

/// <summary>
/// Вспомогательный класс для работы с типами форм
/// </summary>
public static class PaTypeHelper
{
    /// <summary>
    /// Преобразует int в FormType
    /// </summary>
    public static PaType? TryParse(int paTypeId)
    {
        return Enum.IsDefined(typeof(PaType), paTypeId) ? (PaType)paTypeId : null;
    }

    /// <summary>
    /// Проверяет, является ли тип формы типом с одним продуктом
    /// </summary>
    public static bool IsSingleProductType(PaType paType)
    {
        return paType == PaType.SingleProductWithCycleTime ||
               paType == PaType.SingleProductWithWorkstationCapacity;
    }

    /// <summary>
    /// Проверяет, является ли тип формы типом с несколькими продуктами
    /// </summary>
    public static bool IsMultipleProductsType(PaType paType)
    {
        return paType == PaType.MultipleProductsWithCycleTime;
    }

    /// <summary>
    /// Проверяет, является ли тип формы типом с операциями (менее 1 шт. в час)
    /// </summary>
    public static bool IsOperationType(PaType paType)
    {
        return paType == PaType.LessThanOnePerHour;
    }

    /// <summary>
    /// Проверяет, использует ли тип формы время цикла
    /// </summary>
    public static bool UsesCycleTime(PaType paType)
    {
        return paType == PaType.SingleProductWithCycleTime ||
               paType == PaType.MultipleProductsWithCycleTime;
    }

    /// <summary>
    /// Проверяет, использует ли тип формы мощность рабочего места
    /// </summary>
    public static bool UsesWorkstationCapacity(PaType paType)
    {
        return paType == PaType.SingleProductWithWorkstationCapacity;
    }
}