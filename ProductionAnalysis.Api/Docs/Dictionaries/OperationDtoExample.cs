using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class OperationDtoExample : IExamplesProvider<OperationDto>
{
    public OperationDto GetExamples()
    {
        return new OperationDto(
            10,
            "Установка",
            TimeSpan.FromMinutes(10),
            OperationBasedOnType.Operation,
            8,
            null
        );
    }
}

public class EnumerableOperationDtoExample : IExamplesProvider<IEnumerable<OperationDto>>
{
    public IEnumerable<OperationDto> GetExamples()
    {
        return new List<OperationDto>
        {
            new(1, "Установка рамы", TimeSpan.FromMinutes(10), OperationBasedOnType.Nothing, null, null),
            new(2, "Установка", TimeSpan.FromMinutes(15), OperationBasedOnType.Operation, 2, null),
            new(3, "Настройка", TimeSpan.FromMinutes(20), OperationBasedOnType.Product, null, 1)
        };
    }
}