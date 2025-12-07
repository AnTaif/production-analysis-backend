using ProductionAnalysis.Application.Domain;

namespace ProductionAnalysis.Application.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<ICollection<string>> GetRolesAsync(User user);
}