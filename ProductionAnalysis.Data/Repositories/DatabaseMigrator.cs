using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Data.Context;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class DatabaseMigrator(PaDbContext dbContext) : IDatabaseMigrator
{
    public async Task ApplyMigrationsAsync()
    {
        await dbContext.Database.MigrateAsync();
    }
}