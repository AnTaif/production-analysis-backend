namespace ProductionAnalysis.Application.Domain.Forms;

/// <summary>
/// Предоставляет удобный доступ к контексту формы
/// </summary>
public static class FormContextAccessor
{
    private const string ProductContextKey = "product";
    private const string MultiProductContextKey = "multiProduct";
    private const string OperationContextKey = "operation";

    /// <summary>
    /// Получает контекст одного продукта
    /// </summary>
    public static ProductContext? GetProductContext(this Dictionary<string, FormContext> context)
    {
        return context.TryGetValue(ProductContextKey, out var ctx) && ctx is ProductContext productContext
            ? productContext
            : null;
    }

    /// <summary>
    /// Получает контекст нескольких продуктов
    /// </summary>
    public static MultiProductContext? GetMultiProductContext(this Dictionary<string, FormContext> context)
    {
        return context.TryGetValue(MultiProductContextKey, out var ctx) &&
               ctx is MultiProductContext multiProductContext
            ? multiProductContext
            : null;
    }

    /// <summary>
    /// Проверяет, содержит ли контекст один продукт
    /// </summary>
    public static bool HasSingleProduct(this Dictionary<string, FormContext> context)
    {
        return context.ContainsKey(ProductContextKey);
    }

    /// <summary>
    /// Проверяет, содержит ли контекст несколько продуктов
    /// </summary>
    public static bool HasMultipleProducts(this Dictionary<string, FormContext> context)
    {
        return context.ContainsKey(MultiProductContextKey);
    }

    /// <summary>
    /// Получает все продукты из контекста (один или несколько)
    /// </summary>
    public static IReadOnlyCollection<ProductInfo> GetAllProducts(this Dictionary<string, FormContext> context)
    {
        var multiProductContext = context.GetMultiProductContext();
        if (multiProductContext != null)
        {
            return multiProductContext.Products.ToList();
        }

        var productContext = context.GetProductContext();
        if (productContext != null)
        {
            return new List<ProductInfo>
            {
                new(
                    productContext.ProductId,
                    productContext.CycleTime,
                    productContext.WorkstationCapacity,
                    productContext.DailyRate)
            };
        }

        return [];
    }

    /// <summary>
    /// Получает количество продуктов в контексте
    /// </summary>
    public static int GetProductCount(this Dictionary<string, FormContext> context)
    {
        var multiProductContext = context.GetMultiProductContext();
        if (multiProductContext != null)
        {
            return multiProductContext.Products.Count;
        }

        return context.HasSingleProduct() ? 1 : 0;
    }
}