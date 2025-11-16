using ProductionAnalysis.Client.Models.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Auth.ResponseExamples;

public class LoginResponseExample : IExamplesProvider<LoginResponse>
{
    public LoginResponse GetExamples() => new(
        "temp@mail.ru",
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJjM2ZiMTZhMC02NDg4LTRmMzEtYTdjZS1kNzYyNTliMTJlY2IiLCJnaXZlbl9uYW1lIjoi0JjQvNGPIiwiZmFtaWx5X25hbWUiOiLQpNCw0LzQuNC70LjRjyIsImVtYWlsIjoidGVtcEBtYWlsLnJ1Iiwic2lkIjoiMDhkZDdhMTktZjRiYy00NTQxLTg3NDItOWRhM2ZhYTA5ZDczIiwic3ViIjoidGVtcEBtYWlsLnJ1IiwiZXhwIjoxNzQ0NTk1ODQ1LCJpc3MiOiJQSVEiLCJhdWQiOiJQSVEifQ.h_HLJPzwCDLrjAWWr6-If15jmiAjHI9ULT1mdJKkqOY"
    );
}