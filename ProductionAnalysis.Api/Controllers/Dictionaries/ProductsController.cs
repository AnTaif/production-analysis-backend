using Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Dictionaries;
using ProductionAnalysis.Application.Implementation.Dictionaries;
using ProductionAnalysis.Client.Models.Dictionaries;
using Shared.Constants;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers.Dictionaries;

[ApiController]
[Route("dictionaries/products")]
public class ProductsController(
    IDictionariesService dictionariesService,
    IProductsService productsService) : ControllerBase
{
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EnumerableProductDtoExample))]
    [ProducesResponseType<IEnumerable<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts()
    {
        var dtos = await dictionariesService.GetProductsAsync();
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest request)
    {
        var result = await productsService.CreateProductAsync(request);
        return result.ToActionResult(this);
    }

    [HttpPut("{productId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        [FromRoute]
        int productId,
        UpdateProductRequest request)
    {
        var result = await productsService.UpdateProductAsync(productId, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{productId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProduct([FromRoute] int productId)
    {
        var result = await productsService.DeleteProductAsync(productId);
        return result.ToActionResult(this);
    }
}