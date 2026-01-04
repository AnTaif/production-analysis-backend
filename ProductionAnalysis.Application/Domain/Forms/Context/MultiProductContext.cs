namespace ProductionAnalysis.Application.Domain.Forms.Context;

/// <summary>
/// Контекст нескольких продуктов
/// </summary>
public class MultiProductContext : FormContext
{
    public MultiProductContext(ICollection<ProductContext> products)
    {
        Products = products;
    }

    public ICollection<ProductContext> Products { get; }
}