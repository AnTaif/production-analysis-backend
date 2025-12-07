using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Auth;

namespace ProductionAnalysis.Application.Implementation.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest loginRequest);
}

[RegisterScoped]
public class AuthService(
    IUserRepository userRepository,
    ITokenProvider tokenProvider
)
    : IAuthService
{
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return StatusError.NotFound($"User with email {request.Email} not found");
        }

        var isSuccess = await userRepository.CheckPasswordAsync(user, request.Password);
        if (!isSuccess)
        {
            return StatusError.BadRequest("Bad credentials.");
        }

        // Обновляем роли пользователя
        user.Roles = await userRepository.GetRolesAsync(user);
        var token = tokenProvider.GenerateToken(user);

        return new LoginResponse(user.Email, token);
    }
}