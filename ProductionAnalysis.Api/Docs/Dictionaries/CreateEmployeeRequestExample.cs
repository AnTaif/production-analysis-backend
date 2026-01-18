using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class CreateEmployeeRequestExample : IExamplesProvider<CreateEmployeeRequest>
{
    public CreateEmployeeRequest GetExamples()
    {
        return new CreateEmployeeRequest
        {
            FirstName = "Сергей",
            LastName = "Кузнецов",
            MiddleName = "Сергеевич",
            PositionId = 5, // Оператор
            Email = "kuznetsov@mail.ru",
            DepartmentId = 1
        };
    }
}