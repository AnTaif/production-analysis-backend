using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class DowntimeReasonGroupDtoExample : IExamplesProvider<DowntimeReasonGroupDto>
{
    public DowntimeReasonGroupDto GetExamples()
    {
        return new DowntimeReasonGroupDto(
            1,
            "Тех."
        );
    }
}