using ProductionAnalysis.Application.Domain.Forms.Context;

namespace ProductionAnalysis.Application.Domain.Forms;

/// <summary>
/// Расширения для работы с формой
/// </summary>
public static class FormPaTypeExtensions
{
    /// <summary>
    /// Получает контекст одного продукта
    /// </summary>
    public static ProductContext? GetProductContext(this Form form)
    {
        return form.Context.GetProductContext();
    }

    /// <summary>
    /// Получает контекст нескольких продуктов
    /// </summary>
    public static MultiProductContext? GetMultiProductContext(this Form form)
    {
        return form.Context.GetMultiProductContext();
    }

    /// <summary>
    /// Проверяет, содержит ли форма один продукт
    /// </summary>
    public static bool HasSingleProduct(this Form form)
    {
        return form.Context.HasSingleProduct();
    }

    /// <summary>
    /// Проверяет, содержит ли форма несколько продуктов
    /// </summary>
    public static bool HasMultipleProducts(this Form form)
    {
        return form.Context.HasMultipleProducts();
    }

    /// <summary>
    /// Получает все продукты из формы
    /// </summary>
    public static IReadOnlyCollection<ProductContext> GetAllProducts(this Form form)
    {
        return form.Context.GetAllProducts();
    }

    /// <summary>
    /// Получает количество продуктов в форме
    /// </summary>
    public static int GetProductCount(this Form form)
    {
        return form.Context.GetProductCount();
    }
}