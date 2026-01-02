using Microsoft.AspNetCore.Identity;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Converters;
using ProductionAnalysis.Data.Models;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class UserRepository(
    UserManager<UserDbo> userManager
) : IUserRepository
{
    public async Task<User?> FindByEmailAsync(string email)
    {
        var userDbo = await userManager.FindByEmailAsync(email);
        if (userDbo == null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(userDbo);

        return userDbo.ToDomain(roles);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var userDbo = await userManager.FindByIdAsync(userId.ToString());
        if (userDbo == null)
        {
            return false;
        }

        return await userManager.CheckPasswordAsync(userDbo, password);
    }

    public async Task<ICollection<string>> GetRolesAsync(Guid userId)
    {
        var userDbo = await userManager.FindByIdAsync(userId.ToString());
        if (userDbo == null)
        {
            return new List<string>();
        }

        var roles = await userManager.GetRolesAsync(userDbo);
        return roles;
    }
}