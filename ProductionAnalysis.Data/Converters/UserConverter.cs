using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Data.Models;

namespace ProductionAnalysis.Data.Converters;

public static class UserConverter
{
    public static User ToDomain(this UserDbo userDbo, ICollection<string> roles)
    {
        return new User
        {
            Id = userDbo.Id,
            FirstName = userDbo.FirstName,
            LastName = userDbo.LastName,
            MiddleName = userDbo.MiddleName,
            Email = userDbo.Email!,
            Roles = roles
        };
    }
}