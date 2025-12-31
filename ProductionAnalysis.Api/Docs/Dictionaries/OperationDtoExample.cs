using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class OperationDtoExample : IExamplesProvider<OperationDto>
{
    public OperationDto GetExamples()
    {
        return new OperationDto
        {
            Id = 10,
            Name = "Установка",
            Duration = TimeSpan.FromMinutes(10),
            BasedOnType = OperationBasedOnType.Operation,
            BasedOperationId = 8,
            BasedProductId = null
        };
    }
}

public class EnumerableOperationDtoExample : IExamplesProvider<IEnumerable<OperationDto>>
{
    public IEnumerable<OperationDto> GetExamples()
    {
        return new List<OperationDto>
        {
            new()
            {
                Id = 1, Name = "Установка рамы", Duration = TimeSpan.FromMinutes(10),
                BasedOnType = OperationBasedOnType.Nothing, BasedOperationId = null, BasedProductId = null
            },
            new()
            {
                Id = 2, Name = "Установка", Duration = TimeSpan.FromMinutes(15),
                BasedOnType = OperationBasedOnType.Operation, BasedOperationId = 2, BasedProductId = null
            },
            new()
            {
                Id = 3, Name = "Настройка", Duration = TimeSpan.FromMinutes(20),
                BasedOnType = OperationBasedOnType.Product, BasedOperationId = null, BasedProductId = 1
            }
        };
    }
}