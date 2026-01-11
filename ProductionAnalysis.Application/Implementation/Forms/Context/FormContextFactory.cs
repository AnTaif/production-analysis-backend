using ExhaustiveMatching;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Client.Models.Forms;
using PaType = ProductionAnalysis.Application.Domain.Forms.PaType;

namespace ProductionAnalysis.Application.Implementation.Forms.Context;

public interface IFormContextFactory
{
    Dictionary<string, FormContext> CreateContext(CreateFormRequest request);
}

/// <summary>
///     Фабрика для создания контекста формы на основе типа формы
/// </summary>
[RegisterScoped]
public class FormContextFactory : IFormContextFactory
{
    public Dictionary<string, FormContext> CreateContext(CreateFormRequest request)
    {
        var paType = request.PaType.ToDomain();

        return paType switch
        {
            PaType.SingleProductWithCycleTime => CreateSingleProductContextWithCycleTime(request),
            PaType.SingleProductWithWorkstationCapacity => CreateSingleProductContextWithWorkstationCapacity(request),
            PaType.MultipleProductsWithCycleTime => CreateMultipleProductsContextWithCycleTime(request),
            PaType.LessThanOnePerHour => CreateOperationOrProductContext(request),
            PaType.LessThanOnePerShift => CreateOperationOrProductContext(request),
            _ => throw ExhaustiveMatch.Failed(typeof(PaType))
        };
    }

    private static Dictionary<string, FormContext> CreateSingleProductContextWithCycleTime(CreateFormRequest request)
    {
        if (request.Product == null)
            throw new ArgumentException("Product is required for SingleProductWithCycleTime", nameof(request));

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
            throw new ArgumentException("Product is required for SingleProductWithWorkstationCapacity",
                nameof(request));

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
            throw new ArgumentException("Products are required for MultipleProductsWithCycleTime", nameof(request));

        var productContexts = request.Products.Select(p => new ProductContext(
            p.ProductId,
            p.CycleTime,
            null,
            p.DailyRate)).ToList();

        return new Dictionary<string, FormContext>
        {
            ["multiProduct"] = new MultiProductContext(productContexts)
        };
    }

    private static Dictionary<string, FormContext> CreateOperationOrProductContext(CreateFormRequest request)
    {
        if (request.OperationOrProduct == null)
            throw new ArgumentException("OperationOrProduct is required for LessThanOnePerHour/LessThanOnePerShift",
                nameof(request));

        if (!request.OperationOrProduct.OperationId.HasValue && !request.OperationOrProduct.ProductId.HasValue)
            throw new ArgumentException("Either OperationId or ProductId must be set in OperationOrProduct",
                nameof(request));

        if (request.OperationOrProduct.OperationId.HasValue && request.OperationOrProduct.ProductId.HasValue)
            throw new ArgumentException("OperationId and ProductId cannot both be set in OperationOrProduct",
                nameof(request));

        return new Dictionary<string, FormContext>
        {
            ["operationOrProduct"] = new OperationOrProductContext(
                request.OperationOrProduct.OperationId,
                request.OperationOrProduct.ProductId)
        };
    }
}