using Core.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Data.Models;

namespace ProductionAnalysis.Data.Context;

public class PaDbContext(DbContextOptions<PaDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var roles = RolesConstants.GetRoles();
        var identityRoles = roles.Select(role =>
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = role,
                NormalizedName = role.ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });

        modelBuilder.Entity<IdentityRole<Guid>>().HasData(identityRoles);

        base.OnModelCreating(modelBuilder);
    }
}