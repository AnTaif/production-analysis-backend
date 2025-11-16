using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Database;

public abstract class DataSeeder(
    DbContext dbContext,
    ILogger<DataSeeder> logger
    ) : IDataSeeder
{
    public async Task TrySeedAsync()
    {
        logger.LogInformation("Starting database seeding...");

        if (await ShouldSeedAsync())
        {
            await SeedAsync();
            await dbContext.SaveChangesAsync();
            logger.LogWarning("Seeding database completed.");
        }
        else
        {
            logger.LogInformation("Skipping database seeding.");
        }
    }
    
    protected abstract Task<bool> ShouldSeedAsync();

    protected abstract Task SeedAsync();
}