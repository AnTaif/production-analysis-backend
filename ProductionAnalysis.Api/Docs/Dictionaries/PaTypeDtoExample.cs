using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class PaTypeDtoExample : IExamplesProvider<PaTypeDto>
{
    public PaTypeDto GetExamples()
    {
        return new PaTypeDto
        {
            Id = 3,
            Name = "Более 1 шт. в час нескольких номенклатур"
        };
    }
}

public class EnumerablePaTypeDtoExample : IExamplesProvider<IEnumerable<PaTypeDto>>
{
    public IEnumerable<PaTypeDto> GetExamples()
    {
        return new List<PaTypeDto>
        {
            new() { Id = 1, Name = "Более 1 шт. в час (по времени такта)" },
            new() { Id = 2, Name = "Более 1 шт. в час исходя из мощности рабочего  места" },
            new() { Id = 3, Name = "Более 1 шт. в час нескольких номенклатур" }
        };
    }
}