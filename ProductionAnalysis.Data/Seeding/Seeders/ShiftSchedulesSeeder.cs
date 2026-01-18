using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class ShiftSchedulesSeeder(PaDbContext dbContext)
{
    public Task SeedAsync()
    {
        if (dbContext.ShiftSchedules.Any())
            return Task.CompletedTask;

        dbContext.ShiftSchedules.AddRange(
            // Смена 1 (08:00 - 16:00)
            new ShiftScheduleDbo
            {
                Id = 1,
                ShiftId = 1,
                AuxiliaryOperationId = 2, // Перерыв 15 мин
                StartTime = new TimeOnly(10, 0)
            },
            new ShiftScheduleDbo
            {
                Id = 2,
                ShiftId = 1,
                AuxiliaryOperationId = 1, // Обед 30 мин
                StartTime = new TimeOnly(12, 15)
            },
            new ShiftScheduleDbo
            {
                Id = 3,
                ShiftId = 1,
                AuxiliaryOperationId = 2, // Перерыв 15 мин
                StartTime = new TimeOnly(14, 45)
            },
            new ShiftScheduleDbo
            {
                Id = 4,
                ShiftId = 1,
                AuxiliaryOperationId = 3, // Уборка 15 мин
                StartTime = new TimeOnly(17, 00)
            },
            // Смена 2 (16:00 - 00:00)
            new ShiftScheduleDbo
            {
                Id = 5,
                ShiftId = 2,
                AuxiliaryOperationId = 2, // Перерыв 15 мин
                StartTime = new TimeOnly(18, 0)
            },
            new ShiftScheduleDbo
            {
                Id = 6,
                ShiftId = 2,
                AuxiliaryOperationId = 1, // Обед 30 мин
                StartTime = new TimeOnly(20, 15)
            },
            new ShiftScheduleDbo
            {
                Id = 7,
                ShiftId = 2,
                AuxiliaryOperationId = 2, // Перерыв 15 мин
                StartTime = new TimeOnly(22, 45)
            },
            new ShiftScheduleDbo
            {
                Id = 8,
                ShiftId = 2,
                AuxiliaryOperationId = 3, // Уборка 15 мин
                StartTime = new TimeOnly(23, 45)
            },
            // Смена 3 (00:00 - 08:00)
            new ShiftScheduleDbo
            {
                Id = 9,
                ShiftId = 3,
                AuxiliaryOperationId = 2, // Перерыв 15 мин
                StartTime = new TimeOnly(2, 0)
            },
            new ShiftScheduleDbo
            {
                Id = 10,
                ShiftId = 3,
                AuxiliaryOperationId = 1, // Обед 30 мин
                StartTime = new TimeOnly(4, 15)
            },
            new ShiftScheduleDbo
            {
                Id = 11,
                ShiftId = 3,
                AuxiliaryOperationId = 2, // Перерыв 15 мин
                StartTime = new TimeOnly(6, 45)
            },
            new ShiftScheduleDbo
            {
                Id = 12,
                ShiftId = 3,
                AuxiliaryOperationId = 3, // Уборка 15 мин
                StartTime = new TimeOnly(7, 45)
            }
        );

        return Task.CompletedTask;
    }
}