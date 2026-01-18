using Microsoft.AspNetCore.Identity;
using ProductionAnalysis.Data.Context;
using Shared.Constants;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class RolesSeeder(PaDbContext dbContext, RoleManager<IdentityRole<Guid>> roleManager)
{
    public async Task SeedAsync()
    {
        var roles = Roles.GetRoles();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var identityRole = new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = role,
                    NormalizedName = role.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                await roleManager.CreateAsync(identityRole);
            }
        }
    }
}