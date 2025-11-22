using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class ShiftDtoExample : IExamplesProvider<ShiftDto>
{
    public ShiftDto GetExamples()
    {
        return new ShiftDto(
            1,
            "1",
            new TimeOnly(07, 00)
        );
    }
}