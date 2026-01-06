namespace ProductionAnalysis.Application.Domain.Forms.Context;

/// <summary>
/// Предоставляет удобный доступ к контексту формы
/// </summary>
public static class FormContextAccessor
{
    public const string ProductContextKey = "product";
    public const string MultiProductContextKey = "multiProduct";
    public const string OperationContextKey = "operation";

    public static TContext RequireContext<TContext>(this Dictionary<string, FormContext> context, string contextKey)
        where TContext : FormContext
    {
        return context.TryGetValue(contextKey, out var ctx) && ctx is TContext typedContext
            ? typedContext
            : throw new ArgumentException($"Context by key {contextKey} does not exist");
    }

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
    public static IReadOnlyCollection<ProductContext> GetAllProducts(this Dictionary<string, FormContext> context)
    {
        var multiProductContext = context.GetMultiProductContext();
        if (multiProductContext != null)
        {
            return multiProductContext.Products.ToList();
        }

        var productContext = context.GetProductContext();
        if (productContext != null)
        {
            return [productContext];
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

    /// <summary>
    /// Получает контекст операций
    /// </summary>
    public static OperationContext? GetOperationContext(this Dictionary<string, FormContext> context)
    {
        return context.TryGetValue(OperationContextKey, out var ctx) && ctx is OperationContext operationContext
            ? operationContext
            : null;
    }
}