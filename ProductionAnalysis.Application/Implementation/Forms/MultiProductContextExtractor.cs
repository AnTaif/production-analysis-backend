using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IMultiProductContextExtractor
{
    ICollection<ProductContext> Extract(Dictionary<string, FormContextBase>? formContext);
}

[RegisterScoped]
public class MultiProductContextExtractor : IMultiProductContextExtractor
{
    public ICollection<ProductContext> Extract(Dictionary<string, FormContextBase>? formContext)
    {
        if (formContext == null)
        {
            return new List<ProductContext>();
        }

        var products = new List<ProductContext>();

        foreach (var (_, context) in formContext)
        {
            if (context is MultiProductFormContext multiProductContext)
            {
                foreach (var productInfo in multiProductContext.Products)
                {
                    products.Add(new ProductContext
                    {
                        ProductId = productInfo.ProductId,
                        DailyRate = productInfo.DailyRate,
                        CycleTime = productInfo.CycleTime,
                        WorkstationCapacity = productInfo.WorkstationCapacity
                    });
                }
            }
            else if (context is ProductFormContext productContext)
            {
                // Обратная совместимость: один продукт
                products.Add(new ProductContext
                {
                    ProductId = productContext.ProductId,
                    DailyRate = productContext.DailyRate,
                    CycleTime = productContext.CycleTime,
                    WorkstationCapacity = productContext.WorkstationCapacity
                });
            }
        }

        return products;
    }
}