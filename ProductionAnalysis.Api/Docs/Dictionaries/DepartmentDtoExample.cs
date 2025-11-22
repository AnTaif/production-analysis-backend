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

public class EnumerableDepartmentDtoExample : IExamplesProvider<IEnumerable<DepartmentDto>>
{
    public IEnumerable<DepartmentDto> GetExamples()
    {
        return new List<DepartmentDto>
        {
            new(1, "Цех №1", 1),
            new(2, "Цех №2", 1),
            new(3, "Литейный участок", 2)
        };
    }
}