using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class ProductDtoExample : IExamplesProvider<ProductDto>
{
    public ProductDto GetExamples()
    {
        return new ProductDto(
            2,
            "Втулка",
            TimeSpan.FromSeconds(60),
            1
        );
    }
}

public class EnumerableProductDtoExample : IExamplesProvider<IEnumerable<ProductDto>>
{
    public IEnumerable<ProductDto> GetExamples()
    {
        return new List<ProductDto>
        {
            new(1, "Втулка", TimeSpan.FromSeconds(60), 1),
            new(2, "Шайба", TimeSpan.FromSeconds(120), 1),
            new(3, "Деталь с предприятия 2", TimeSpan.FromSeconds(30), 2)
        };
    }
}