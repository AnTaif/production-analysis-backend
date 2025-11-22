using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class EmployeeDtoExample : IExamplesProvider<EmployeeDto>
{
    public EmployeeDto GetExamples()
    {
        return new EmployeeDto(
            15,
            "Иванов Иван Иванович",
            "Бригадир",
            2
        );
    }
}