using ProductionAnalysis.Client.Models.Dictionaries;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Dictionaries;

public class UpdateEmployeeRequestExample : IExamplesProvider<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequest GetExamples()
    {
        return new UpdateEmployeeRequest
        {
            FirstName = "Сергей",
            LastName = "Кузнецов",
            MiddleName = "Сергеевич",
            PositionId = 6, // Старший оператор
            Email = "kuznetsov@mail.ru",
            DepartmentId = 1
        };
    }
}