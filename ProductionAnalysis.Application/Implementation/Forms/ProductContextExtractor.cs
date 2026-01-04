using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IProductContextExtractor
{
    ProductContext? Extract(Dictionary<string, FormContextBase>? formContext);
}

[RegisterScoped]
public class ProductContextExtractor : IProductContextExtractor
{
    public ProductContext? Extract(Dictionary<string, FormContextBase>? formContext)
    {
        if (formContext == null)
        {
            return null;
        }

        foreach (var (_, context) in formContext)
        {
            if (context is ProductFormContext productContext)
            {
                return new ProductContext
                {
                    ProductId = productContext.ProductId,
                    DailyRate = productContext.DailyRate,
                    CycleTime = productContext.CycleTime,
                    WorkstationCapacity = productContext.WorkstationCapacity
                };
            }
        }

        return null;
    }
}