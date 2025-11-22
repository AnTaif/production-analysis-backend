using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class PaTypeDtoExample : IExamplesProvider<PaTypeDto>
{
    public PaTypeDto GetExamples()
    {
        return new PaTypeDto(
            3,
            "Более 1 шт. в час нескольких номенклатур"
        );
    }
}

public class EnumerablePaTypeDtoExample : IExamplesProvider<IEnumerable<PaTypeDto>>
{
    public IEnumerable<PaTypeDto> GetExamples()
    {
        return new List<PaTypeDto>
        {
            new(1, "Более 1 шт. в час (по времени такта)"),
            new(2, "Более 1 шт. в час исходя из мощности рабочего  места"),
            new(3, "Более 1 шт. в час нескольких номенклатур")
        };
    }
}