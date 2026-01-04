using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class EmployeeDtoExample : IExamplesProvider<EmployeeDto>
{
    public EmployeeDto GetExamples()
    {
        return new EmployeeDto
        {
            Id = 15,
            FullName = "Иванов Иван Иванович",
            Position = "Бригадир",
            DepartmentId = 2,
            UserId = Guid.NewGuid()
        };
    }
}

public class EnumerableEmployeeDtoExample : IExamplesProvider<IEnumerable<EmployeeDto>>
{
    public IEnumerable<EmployeeDto> GetExamples()
    {
        return new List<EmployeeDto>
        {
            new() { Id = 1, FullName = "Иван Иванов Иванович", Position = "Оператор", DepartmentId = 1, UserId = null },
            new()
            {
                Id = 2, FullName = "Пётр Петров Петрович", Position = "Старший оператор", DepartmentId = 1,
                UserId = Guid.NewGuid()
            },
            new()
            {
                Id = 3, FullName = "Алексей Сидоров Алексеевич", Position = "Мастер участка", DepartmentId = 2,
                UserId = Guid.NewGuid()
            },
        };
    }
}