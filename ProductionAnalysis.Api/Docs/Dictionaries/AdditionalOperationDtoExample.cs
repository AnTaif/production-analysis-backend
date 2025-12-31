using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class AdditionalOperationDtoExample : IExamplesProvider<AdditionalOperationDto>
{
    public AdditionalOperationDto GetExamples()
    {
        return new AdditionalOperationDto
        {
            Id = 1,
            Name = "Обед 30 мин",
            Duration = TimeSpan.FromMinutes(30)
        };
    }
}

public class EnumerableAdditionalOperationDtoExample : IExamplesProvider<IEnumerable<AdditionalOperationDto>>
{
    public IEnumerable<AdditionalOperationDto> GetExamples()
    {
        return new List<AdditionalOperationDto>
        {
            new() { Id = 1, Name = "Обед 30 мин", Duration = TimeSpan.FromMinutes(30) },
            new() { Id = 1, Name = "Переналадка 15 мин", Duration = TimeSpan.FromMinutes(15) }
        };
    }
}