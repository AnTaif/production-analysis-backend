using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormContextFactory
{
    Dictionary<string, FormContext> CreateContext(CreateFormRequest request);
}

/// <summary>
/// Фабрика для создания контекста формы на основе типа формы
/// </summary>
[RegisterScoped]
public class FormContextFactory : IFormContextFactory
{
    public Dictionary<string, FormContext> CreateContext(CreateFormRequest request)
    {
        var paType = PaTypeHelper.TryParse(request.PaTypeId);
        if (paType == null)
        {
            throw new NotSupportedException($"Unknown form type: {request.PaTypeId}");
        }

        return paType.Value switch
        {
            PaType.SingleProductWithCycleTime => CreateSingleProductContextWithCycleTime(request),
            PaType.SingleProductWithWorkstationCapacity => CreateSingleProductContextWithWorkstationCapacity(request),
            PaType.MultipleProductsWithCycleTime => CreateMultipleProductsContextWithCycleTime(request),
            _ => throw new NotSupportedException($"Unsupported form type: {paType.Value}")
        };
    }

    private static Dictionary<string, FormContext> CreateSingleProductContextWithCycleTime(CreateFormRequest request)
    {
        if (request.Product == null)
        {
            throw new ArgumentException("Product is required for SingleProductWithCycleTime", nameof(request));
        }

        return new Dictionary<string, FormContext>
        {
            ["product"] = new ProductContext(
                request.Product.ProductId,
                request.Product.CycleTime,
                null,
                request.Product.DailyRate)
        };
    }

    private static Dictionary<string, FormContext> CreateSingleProductContextWithWorkstationCapacity(
        CreateFormRequest request)
    {
        if (request.Product == null)
        {
            throw new ArgumentException("Product is required for SingleProductWithWorkstationCapacity",
                nameof(request));
        }

        return new Dictionary<string, FormContext>
        {
            ["product"] = new ProductContext(
                request.Product.ProductId,
                null,
                request.Product.WorkstationCapacity,
                request.Product.DailyRate)
        };
    }

    private static Dictionary<string, FormContext> CreateMultipleProductsContextWithCycleTime(CreateFormRequest request)
    {
        if (request.Products == null || request.Products.Count == 0)
        {
            throw new ArgumentException("Products are required for MultipleProductsWithCycleTime", nameof(request));
        }

        var productInfos = request.Products.Select(p => new ProductInfo(
            p.ProductId,
            p.CycleTime,
            null,
            p.DailyRate)).ToList();

        return new Dictionary<string, FormContext>
        {
            ["multiProduct"] = new MultiProductContext(productInfos)
        };
    }
}