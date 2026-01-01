using ProductionAnalysis.Client.Models.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Auth.RequestExamples;

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples() => new()
    {
        Email = "departmenthead@mail.ru",
        Password = "password"
    };
}