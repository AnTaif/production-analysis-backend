using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class DepartmentDtoExample : IExamplesProvider<DepartmentDto>
{
    public DepartmentDto GetExamples()
    {
        return new DepartmentDto
        {
            Id = 2,
            Name = "Участок 1",
            EnterpriseId = 1
        };
    }
}

public class EnumerableDepartmentDtoExample : IExamplesProvider<IEnumerable<DepartmentDto>>
{
    public IEnumerable<DepartmentDto> GetExamples()
    {
        return new List<DepartmentDto>
        {
            new() { Id = 1, Name = "Цех №1", EnterpriseId = 1 },
            new() { Id = 2, Name = "Цех №2", EnterpriseId = 1 },
            new() { Id = 3, Name = "Литейный участок", EnterpriseId = 2 }
        };
    }
}