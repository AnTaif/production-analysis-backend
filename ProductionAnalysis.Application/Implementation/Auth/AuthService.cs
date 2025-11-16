using Core.Results;
using Microsoft.AspNetCore.Identity;
using ProductionAnalysis.Client.Models.Auth;
using ProductionAnalysis.Data.Models;

namespace ProductionAnalysis.Application.Implementation.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest loginRequest);
}

[RegisterScoped]
public class AuthService(
    UserManager<UserDbo> userManager,
    ITokenProvider tokenProvider
)
    : IAuthService
{
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return StatusError.NotFound($"User with email {request.Email} not found");
        }

        var isSuccess = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isSuccess)
        {
            return StatusError.BadRequest("Bad credentials.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenProvider.GenerateToken(user, roles);

        return new LoginResponse(user.Email!, token);
    }
}