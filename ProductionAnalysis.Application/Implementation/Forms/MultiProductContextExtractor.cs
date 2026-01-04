using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IMultiProductContextExtractor
{
    ICollection<ProductInfo> Extract(Dictionary<string, FormContext>? formContext);
}

[RegisterScoped]
public class MultiProductContextExtractor : IMultiProductContextExtractor
{
    public ICollection<ProductInfo> Extract(Dictionary<string, FormContext>? formContext)
    {
        if (formContext == null)
        {
            return new List<ProductInfo>();
        }

        var allProducts = formContext.GetAllProducts();
        return allProducts.ToList();
    }
}