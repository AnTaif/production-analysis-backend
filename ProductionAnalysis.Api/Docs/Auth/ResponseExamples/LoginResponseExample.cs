using ProductionAnalysis.Client.Models.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Auth.ResponseExamples;

public class LoginResponseExample : IExamplesProvider<LoginResponse>
{
    public LoginResponse GetExamples() => new()
    {
        Email = "departmenthead@mail.ru",
        Token = "token-string"
    };
}