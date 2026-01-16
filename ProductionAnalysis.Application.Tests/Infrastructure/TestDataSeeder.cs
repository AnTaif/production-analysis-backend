using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Seeding;

namespace ProductionAnalysis.Application.Tests.Infrastructure;

public class TestDataSeeder(
    PaDbContext dbContext,
    UserManager<UserDbo> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ILogger<PaDataSeeder> logger)
{
    private readonly PaDataSeeder paDataSeeder = new(dbContext, userManager, roleManager, logger);

    public async Task SeedAllAsync()
    {
        await paDataSeeder.ForceSeedAsync();
    }
}