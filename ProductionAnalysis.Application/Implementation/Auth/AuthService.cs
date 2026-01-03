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
    IPaUnitOfWork unitOfWork,
    ITokenProvider tokenProvider
)
    : IAuthService
{
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await unitOfWork.Users.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ServiceError.NotFound($"User with email {request.Email} not found");
        }

        var isSuccess = await unitOfWork.Users.CheckPasswordAsync(user.Id, request.Password);
        if (!isSuccess)
        {
            return ServiceError.BadRequest("Bad credentials.");
        }

        var token = tokenProvider.ProvideToken(user);

        return new LoginResponse
        {
            Email = user.Email,
            Token = token
        };
    }
}