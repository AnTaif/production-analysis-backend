using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class ProductDtoExample : IExamplesProvider<ProductDto>
{
    public ProductDto GetExamples()
    {
        return new ProductDto
        {
            Id = 2,
            Name = "Втулка",
            TactTime = TimeSpan.FromSeconds(60),
            EnterpriseId = 1
        };
    }
}

public class EnumerableProductDtoExample : IExamplesProvider<IEnumerable<ProductDto>>
{
    public IEnumerable<ProductDto> GetExamples()
    {
        return new List<ProductDto>
        {
            new() { Id = 1, Name = "Втулка", TactTime = TimeSpan.FromSeconds(60), EnterpriseId = 1 },
            new() { Id = 2, Name = "Шайба", TactTime = TimeSpan.FromSeconds(120), EnterpriseId = 1 },
            new() { Id = 3, Name = "Деталь с предприятия 2", TactTime = TimeSpan.FromSeconds(30), EnterpriseId = 2 }
        };
    }
}