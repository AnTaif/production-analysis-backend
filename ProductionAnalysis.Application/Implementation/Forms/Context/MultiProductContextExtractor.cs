using ProductionAnalysis.Application.Domain.Forms.Context;

namespace ProductionAnalysis.Application.Implementation.Forms.Context;

public interface IMultiProductContextExtractor
{
    ICollection<ProductContext> Extract(Dictionary<string, FormContext>? formContext);
}

[RegisterScoped]
public class MultiProductContextExtractor : IMultiProductContextExtractor
{
    public ICollection<ProductContext> Extract(Dictionary<string, FormContext>? formContext)
    {
        if (formContext == null)
        {
            return new List<ProductContext>();
        }

        var allProducts = formContext.GetAllProducts();
        return allProducts.ToList();
    }
}