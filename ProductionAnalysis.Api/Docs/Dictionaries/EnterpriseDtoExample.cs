using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class EnterpriseDtoExample : IExamplesProvider<EnterpriseDto>
{
    public EnterpriseDto GetExamples()
    {
        return new EnterpriseDto(
            1,
            "Предприятие 1"
        );
    }
}