using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class EnterpriseDtoExample : IExamplesProvider<EnterpriseDto>
{
    public EnterpriseDto GetExamples()
    {
        return new EnterpriseDto
        {
            Id = 1,
            Name = "Предприятие №1"
        };
    }
}

public class EnumerableEnterpriseDtoExample : IExamplesProvider<IEnumerable<EnterpriseDto>>
{
    public IEnumerable<EnterpriseDto> GetExamples()
    {
        return new List<EnterpriseDto>
        {
            new() { Id = 1, Name = "Предприятие №1" },
            new() { Id = 2, Name = "Завод в свердловской области" }
        };
    }
}