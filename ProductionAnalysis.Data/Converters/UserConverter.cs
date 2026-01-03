using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Data.Models;

namespace ProductionAnalysis.Data.Converters;

public static class UserConverter
{
    public static User ToDomain(this UserDbo userDbo, ICollection<string> roles)
    {
        return new User(
            userDbo.Id,
            userDbo.FirstName,
            userDbo.LastName,
            userDbo.MiddleName,
            userDbo.Email!,
            roles
        );
    }
}