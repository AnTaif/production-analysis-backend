using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class AdditionalOperationDtoExample : IExamplesProvider<AdditionalOperationDto>
{
    public AdditionalOperationDto GetExamples()
    {
        return new AdditionalOperationDto(1, "Обед 30 мин", TimeSpan.FromMinutes(30));
    }
}

public class EnumerableAdditionalOperationDtoExample : IExamplesProvider<IEnumerable<AdditionalOperationDto>>
{
    public IEnumerable<AdditionalOperationDto> GetExamples()
    {
        return new List<AdditionalOperationDto>
        {
            new(1, "Обед 30 мин", TimeSpan.FromMinutes(30)),
            new(1, "Переналадка 15 мин", TimeSpan.FromMinutes(15))
        };
    }
}