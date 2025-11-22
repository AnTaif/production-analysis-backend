using Core.Database;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Data.Context;

namespace ProductionAnalysis.Application.Implementation.Admin;

public interface IDbMaintenanceService
{
    Task ApplyMigrationsAsync();
}

public class DbMaintenanceService(
    PaDbContext dbContext,
    IDataSeeder dataSeeder
)
    : IDbMaintenanceService
{
    public async Task ApplyMigrationsAsync()
    {
        await dbContext.Database.MigrateAsync();
        await dataSeeder.ForceSeedAsync();
    }
}