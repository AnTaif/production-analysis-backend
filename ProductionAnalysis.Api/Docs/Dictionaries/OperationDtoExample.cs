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