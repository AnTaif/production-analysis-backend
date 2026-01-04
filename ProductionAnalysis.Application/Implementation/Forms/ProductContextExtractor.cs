using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IProductContextExtractor
{
    ProductContext? Extract(Dictionary<string, FormContext>? formContext);
}

[RegisterScoped]
public class ProductContextExtractor : IProductContextExtractor
{
    public ProductContext? Extract(Dictionary<string, FormContext>? formContext)
    {
        return formContext?.GetProductContext();
    }
}