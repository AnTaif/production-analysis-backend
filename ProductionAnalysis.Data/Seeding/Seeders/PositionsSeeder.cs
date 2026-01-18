using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models.Dictionaries;
using Shared.Constants;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class PositionsSeeder(PaDbContext dbContext)
{
    public Task SeedAsync()
    {
        if (dbContext.Positions.Any())
            return Task.CompletedTask;

        dbContext.Positions.AddRange(
            new PositionDbo
            {
                Id = 1,
                Name = "Оператор",
                Role = Roles.Operator
            },
            new PositionDbo
            {
                Id = 2,
                Name = "Начальник участка",
                Role = Roles.DepartmentHead
            },
            new PositionDbo
            {
                Id = 3,
                Name = "Аналитик",
                Role = Roles.Analyst
            },
            new PositionDbo
            {
                Id = 4,
                Name = "Администратор",
                Role = Roles.Admin
            },
            new PositionDbo
            {
                Id = 7,
                Name = "Технолог",
                Role = Roles.JustEmployee
            },
            new PositionDbo
            {
                Id = 8,
                Name = "Инженер",
                Role = Roles.JustEmployee
            },
            new PositionDbo
            {
                Id = 10,
                Name = "Наладчик",
                Role = Roles.JustEmployee
            },
            new PositionDbo
            {
                Id = 11,
                Name = "Сварщик",
                Role = Roles.JustEmployee
            },
            new PositionDbo
            {
                Id = 12,
                Name = "Токарь",
                Role = Roles.JustEmployee
            }
        );

        return Task.CompletedTask;
    }
}