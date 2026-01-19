using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IProductsService
{
    Task<Result<ProductDto>> CreateProductAsync(CreateProductRequest request);
    Task<Result<ProductDto>> UpdateProductAsync(int productId, UpdateProductRequest request);
    Task<Result> DeleteProductAsync(int productId);
}

[RegisterScoped]
public class ProductsService(IPaUnitOfWork unitOfWork) : IProductsService
{
    public async Task<Result<ProductDto>> CreateProductAsync(CreateProductRequest request)
    {
        var enterpriseExists = await unitOfWork.Dictionaries.EnterpriseExistsAsync(request.EnterpriseId);
        if (!enterpriseExists)
        {
            return ServiceError.NotFound($"Enterprise with id {request.EnterpriseId} not found");
        }

        var product = await unitOfWork.Dictionaries.CreateProductAsync(request);
        return product;
    }

    public async Task<Result<ProductDto>> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        var existingProduct = await unitOfWork.Dictionaries.FindProductByIdAsync(productId);
        if (existingProduct == null)
        {
            return ServiceError.NotFound($"Product with id {productId} not found");
        }

        var enterpriseExists = await unitOfWork.Dictionaries.EnterpriseExistsAsync(request.EnterpriseId);
        if (!enterpriseExists)
        {
            return ServiceError.NotFound($"Enterprise with id {request.EnterpriseId} not found");
        }

        var updatedProduct = await unitOfWork.Dictionaries.UpdateProductAsync(productId, request);
        if (updatedProduct == null)
        {
            return ServiceError.NotFound($"Product with id {productId} not found");
        }

        return updatedProduct;
    }

    public async Task<Result> DeleteProductAsync(int productId)
    {
        var product = await unitOfWork.Dictionaries.FindProductByIdAsync(productId);
        if (product == null)
        {
            return ServiceError.NotFound($"Product with id {productId} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeleteProductAsync(productId);
        if (!deleted)
        {
            return ServiceError.NotFound($"Product with id {productId} not found");
        }

        return Result.Success;
    }
}