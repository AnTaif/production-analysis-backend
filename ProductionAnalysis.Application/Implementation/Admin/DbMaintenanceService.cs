using Core.Database;
using ProductionAnalysis.Application.Repositories;

namespace ProductionAnalysis.Application.Implementation.Admin;

public interface IDbMaintenanceService
{
    Task ApplyMigrationsAsync();
}

[RegisterScoped]
public class DbMaintenanceService(
    IDatabaseMigrator databaseMigrator,
    IDataSeeder dataSeeder
)
    : IDbMaintenanceService
{
    public async Task ApplyMigrationsAsync()
    {
        await databaseMigrator.ApplyMigrationsAsync();
        await dataSeeder.ForceSeedAsync();
    }
}