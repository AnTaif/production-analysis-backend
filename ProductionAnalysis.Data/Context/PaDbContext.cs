using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Context;

public class PaDbContext(DbContextOptions<PaDbContext> options)
    : IdentityDbContext<UserDbo, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<FormDbo> Forms { get; set; }
    
    #region Dictionaries

    public DbSet<DepartmentDbo> Departments { get; set; }
    public DbSet<DowntimeReasonGroupDbo> DowntimeReasonGroups { get; set; }
    public DbSet<EmployeeDbo> Employees { get; set; }
    public DbSet<EnterpriseDbo> Enterprises { get; set; }
    public DbSet<AdditionalOperationDbo> AdditionalOperations { get; set; }
    public DbSet<OperationDbo> Operations { get; set; }
    public DbSet<PaTypeDbo> PaTypes { get; set; }
    public DbSet<ProductDbo> Products { get; set; }
    public DbSet<ShiftDbo> Shifts { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}