namespace ProductionAnalysis.Application.Repositories;

public interface IDatabaseMigrator
{
    Task ApplyMigrationsAsync();
}