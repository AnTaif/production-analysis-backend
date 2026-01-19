using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class OperationsSeeder(PaDbContext dbContext)
{
    public Task SeedAsync()
    {
        if (dbContext.Operations.Any())
            return Task.CompletedTask;

        dbContext.Operations.AddRange(
            // Операции для продукта Втулка
            new OperationDbo
            {
                Id = 1,
                Name = "Подготовка",
                DurationInSeconds = 300,
                BasedOnType = 3,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 2,
                Name = "Обработка",
                DurationInSeconds = 900,
                BasedOnType = 3,
                BasedProductId = 1,
            },
            new OperationDbo
            {
                Id = 3,
                Name = "Сборка",
                DurationInSeconds = 1200,
                BasedOnType = 3,
                BasedProductId = 1
            },

            // Операция "Установка прибора" с под-операциями
            new OperationDbo
            {
                Id = 7,
                Name = "Установка прибора",
                DurationInSeconds = null,
                BasedOnType = 1,
                BasedOperationId = null,
                BasedProductId = null
            },
            // Под-операции для "Установка прибора"
            new OperationDbo
            {
                Id = 8,
                Name = "1. Подсборка",
                DurationInSeconds = 25 * 60,
                BasedOnType = 2,
                BasedOperationId = 7,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 9,
                Name = "2. Установка детали 1",
                DurationInSeconds = 15 * 60,
                BasedOnType = 2,
                BasedOperationId = 7,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 10,
                Name = "3. Установка детали 2",
                DurationInSeconds = 20 * 60,
                BasedOnType = 2,
                BasedOperationId = 7,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 11,
                Name = "4. Установка детали 3",
                DurationInSeconds = 30 * 60,
                BasedOnType = 2,
                BasedOperationId = 7,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 12,
                Name = "5. Настройка",
                DurationInSeconds = 20 * 60,
                BasedOnType = 2,
                BasedOperationId = 7,
                BasedProductId = null
            },

            // Базовая операция
            new OperationDbo
            {
                Id = 21,
                Name = "Сборка кабины автопогрузчика",
                DurationInSeconds = 9999999,
                BasedOnType = 1,
                BasedOperationId = null,
                BasedProductId = null,
            },
            // Под-операции 21
            new OperationDbo
            {
                Id = 22,
                Name = "1. Установка рамы",
                DurationInSeconds = 60 * 55,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 23,
                Name = "2. Установка гидросистемы",
                DurationInSeconds = 60 * 35,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 24,
                Name = "3. Установка двигателя",
                DurationInSeconds = 60 * 30,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 25,
                Name = "4. Установка переднего моста",
                DurationInSeconds = 60 * 40,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 26,
                Name = "5. Установка кабины",
                DurationInSeconds = 60 * 40,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 27,
                Name = "6. Подключение аппаратуры",
                DurationInSeconds = 60 * 40,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 28,
                Name = "7. Соединение шарнина с рамой",
                DurationInSeconds = 60 * 45,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 29,
                Name = "8. Установка гидроцилиндров",
                DurationInSeconds = 60 * 135,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 30,
                Name = "9. Разводка электирики",
                DurationInSeconds = 60 * 50,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 31,
                Name = "10. Подключение электирики",
                DurationInSeconds = 60 * 40,
                BasedOnType = 2,
                BasedOperationId = 21,
                BasedProductId = null
            }
        );

        return Task.CompletedTask;
    }
}