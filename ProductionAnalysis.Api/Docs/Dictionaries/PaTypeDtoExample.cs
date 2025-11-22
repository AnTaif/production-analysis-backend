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