using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class DepartmentDtoExample : IExamplesProvider<DepartmentDto>
{
    public DepartmentDto GetExamples()
    {
        return new DepartmentDto(
            2,
            "Участок 1",
            1
        );
    }
}