using Core.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context;

public class PaDbContext(DbContextOptions<PaDbContext> options) : IdentityDbContext<UserDbo, IdentityRole<Guid>, Guid>(options)
{
    #region Dictionaries

    public DbSet<DepartmentDbo> Departments { get; set; }
    public DbSet<DowntimeReasonGroupDbo> DowntimeReasonGroups { get; set; }
    public DbSet<EmployeeDbo> Employees { get; set; }
    public DbSet<EnterpriseDbo> Enterprises { get; set; }
    public DbSet<OperationDbo> Operations { get; set; }
    public DbSet<PaTypeDbo> PaTypes { get; set; }
    public DbSet<ShiftDbo> Shifts { get; set; }

    #endregion
    
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