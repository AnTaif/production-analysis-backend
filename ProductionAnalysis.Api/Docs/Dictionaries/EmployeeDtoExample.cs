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

public class EnumerableEmployeeDtoExample : IExamplesProvider<IEnumerable<EmployeeDto>>
{
    public IEnumerable<EmployeeDto> GetExamples()
    {
        return new List<EmployeeDto>
        {
            new(1, "Иван Иванов Иванович", "Оператор", 1),
            new(2, "Пётр Петров Петрович", "Старший оператор", 1),
            new(3, "Алексей Сидоров Алексеевич", "Мастер участка", 2)
        };
    }
}