using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class ProductsSeeder(PaDbContext dbContext)
{
    public Task SeedAsync()
    {
        if (dbContext.Products.Any())
            return Task.CompletedTask;

        dbContext.Products.AddRange(
            new ProductDbo
            {
                Id = 1,
                Name = "Втулка",
                TactTimeInSeconds = 60,
                EnterpriseId = 1
            },
            new ProductDbo
            {
                Id = 2,
                Name = "Шайба",
                TactTimeInSeconds = 30,
                EnterpriseId = 1
            },
            new ProductDbo
            {
                Id = 3,
                Name = "Подшипник",
                TactTimeInSeconds = 60,
                EnterpriseId = 1
            },
            new ProductDbo
            {
                Id = 4,
                Name = "Фланец",
                TactTimeInSeconds = 60,
                EnterpriseId = 1
            }
        );

        return Task.CompletedTask;
    }
}