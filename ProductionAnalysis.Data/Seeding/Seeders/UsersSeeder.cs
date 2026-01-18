using Microsoft.AspNetCore.Identity;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using Shared.Constants;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class UsersSeeder(PaDbContext dbContext, UserManager<UserDbo> userManager)
{
    public async Task SeedAsync()
    {
        var @operator = new UserDbo
        {
            Id = Guid.NewGuid(),
            Email = "operator@mail.ru",
            UserName = "operator@mail.ru",
            FirstName = "Operator",
            LastName = "LastName",
            MiddleName = "MiddleName"
        };

        var departmentHead = new UserDbo
        {
            Id = Guid.NewGuid(),
            Email = "departmentHead@mail.ru",
            UserName = "departmentHead@mail.ru",
            FirstName = "departmentHead",
            LastName = "LastName",
            MiddleName = "MiddleName"
        };

        var analyst = new UserDbo
        {
            Id = Guid.NewGuid(),
            Email = "analyst@mail.ru",
            UserName = "analyst@mail.ru",
            FirstName = "Analyst",
            LastName = "LastName",
            MiddleName = "MiddleName"
        };

        var admin = new UserDbo
        {
            Id = Guid.NewGuid(),
            Email = "admin@mail.ru",
            UserName = "admin@mail.ru",
            FirstName = "Admin",
            LastName = "LastName",
            MiddleName = "MiddleName"
        };

        var users = new List<(UserDbo, string[])>
        {
            (@operator, [Roles.Operator]),
            (departmentHead, [Roles.DepartmentHead]),
            (analyst, [Roles.Analyst]),
            (admin, [Roles.Admin]),
        };

        await CreateUsersAsync(users);
    }

    private async Task CreateUsersAsync(IEnumerable<(UserDbo, string[])> users)
    {
        foreach (var (user, roles) in users)
        {
            var result = await userManager.CreateAsync(user, "password");

            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(user, roles);
            }
        }
    }
}