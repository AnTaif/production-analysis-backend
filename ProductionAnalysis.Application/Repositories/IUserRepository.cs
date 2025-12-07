using ProductionAnalysis.Application.Domain;

namespace ProductionAnalysis.Application.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    Task<ICollection<string>> GetRolesAsync(Guid userId);
}