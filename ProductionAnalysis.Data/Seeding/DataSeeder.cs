using Core.Auth;
using Core.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using Serilog;

namespace ProductionAnalysis.Data.Seeding;

public class DataSeeder(
    PaDbContext dbContext,
    UserManager<UserDbo> userManager,
    ILogger<DataSeeder> logger
)
    : IDataSeeder
{
    public async Task<bool> TrySeedAsync()
    {
        logger.LogInformation("Starting database seeding...");

        if (!await dbContext.Database.EnsureCreatedAsync() && dbContext.Users.Any())
        {
            logger.LogInformation("Database already has some data, skipping...");
            return false;
        }

        await SeedUsersAsync();
        await dbContext.SaveChangesAsync();
        logger.LogWarning("Database seeding completed.");
        return true;
    }

    private async Task SeedUsersAsync()
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
            (@operator, [RolesConstants.Operator]),
            (analyst, [RolesConstants.Analyst]),
            (admin, [RolesConstants.Admin]),
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