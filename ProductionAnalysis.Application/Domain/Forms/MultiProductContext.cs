namespace ProductionAnalysis.Application.Domain.Forms;

/// <summary>
/// Контекст нескольких продуктов
/// </summary>
public class MultiProductContext : FormContext
{
    public MultiProductContext(ICollection<ProductInfo> products)
    {
        Products = products;
    }

    public ICollection<ProductInfo> Products { get; }
}