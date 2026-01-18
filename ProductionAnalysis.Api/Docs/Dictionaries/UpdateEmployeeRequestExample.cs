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
            Position = "Старший оператор",
            Email = "kuznetsov@mail.ru",
            DepartmentId = 1
        };
    }
}