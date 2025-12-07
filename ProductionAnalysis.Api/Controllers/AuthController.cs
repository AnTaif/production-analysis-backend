using Core.Results;
using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Api.Docs.Auth.RequestExamples;
using ProductionAnalysis.Api.Docs.Auth.ResponseExamples;
using ProductionAnalysis.Application.Implementation.Auth;
using ProductionAnalysis.Client.Models.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginResponseExample))]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest loginRequest)
    {
        var result = await authService.LoginAsync(loginRequest);
        return result.ToActionResult(this);
    }
}