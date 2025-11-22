using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class EnterpriseDtoExample : IExamplesProvider<EnterpriseDto>
{
    public EnterpriseDto GetExamples()
    {
        return new EnterpriseDto(
            1,
            "Предприятие №1"
        );
    }
}

public class EnumerableEnterpriseDtoExample : IExamplesProvider<IEnumerable<EnterpriseDto>>
{
    public IEnumerable<EnterpriseDto> GetExamples()
    {
        return new List<EnterpriseDto>
        {
            new(1, "Предприятие №1"),
            new(2, "Завод в свердловской области")
        };
    }
}