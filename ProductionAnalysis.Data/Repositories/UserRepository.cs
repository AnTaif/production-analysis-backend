using Microsoft.AspNetCore.Identity;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Repositories;
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
        return new User
        {
            Id = userDbo.Id,
            FirstName = userDbo.FirstName,
            LastName = userDbo.LastName,
            MiddleName = userDbo.MiddleName,
            Email = userDbo.Email!,
            Roles = roles.ToList()
        };
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        var userDbo = await userManager.FindByEmailAsync(user.Email);
        if (userDbo == null)
        {
            return false;
        }

        return await userManager.CheckPasswordAsync(userDbo, password);
    }

    public async Task<ICollection<string>> GetRolesAsync(User user)
    {
        var userDbo = await userManager.FindByEmailAsync(user.Email);
        if (userDbo == null)
        {
            return new List<string>();
        }

        var roles = await userManager.GetRolesAsync(userDbo);
        return roles.ToList();
    }
}