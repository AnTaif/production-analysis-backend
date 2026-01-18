using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class IndicatorsSeeder(PaDbContext dbContext)
{
    public async Task SeedAsync()
    {
        if (dbContext.Indicators.Any())
            return;

        dbContext.Indicators.AddRange(
            new IndicatorDbo
            {
                Id = 1,
                Name = "Время работы, час",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 2,
                Name = "План, шт",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = "",
                Formula = null,
                HasSummation = true,
            },
            new IndicatorDbo
            {
                Id = 3,
                Name = "Факт, шт",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = "",
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 4,
                Name = "Отклонен, шт",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Formula,
                ValueSelector = "",
                Formula = "indicator_3 - indicator_2",
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 5,
                Name = "Простой, мин",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 6,
                Name = "Ответственный за простой",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Dictionary,
                ValueSelector = "employees",
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 7,
                Name = "Группы причин",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Dictionary,
                ValueSelector = "downtime-reason-groups",
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 8,
                Name = "Причины отклонения, принятые меры",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 10,
                Name = "Наименование операции",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Dictionary,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 11,
                Name = "Время операции, мин",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Context,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 12,
                Name = "Время начала план",
                ValueType = FieldValueTypes.Time,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 13,
                Name = "Время начала факт",
                ValueType = FieldValueTypes.Time,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 14,
                Name = "Время окончания план",
                ValueType = FieldValueTypes.Time,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 15,
                Name = "Время окончания факт",
                ValueType = FieldValueTypes.Time,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 16,
                Name = "План, мин",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 17,
                Name = "Факт, мин",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 18,
                Name = "Отклонен, мин",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Formula,
                ValueSelector = null,
                Formula = "(indicator_15 - indicator_13) - (indicator_14 - indicator_12)",
                HasSummation = true
            },
            // Накопительные индикаторы
            new IndicatorDbo
            {
                Id = 19,
                Name = "План накоп, шт",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "2", // ID базового индикатора
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 20,
                Name = "Факт накоп, шт",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "3", // ID базового индикатора
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 21,
                Name = "Отклонен накоп, шт",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "4", // ID базового индикатора
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 22,
                Name = "Отклонен накоп, мин",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "18", // ID базового индикатора
                Formula = null,
                HasSummation = false
            }
        );

        await dbContext.SaveChangesAsync();
    }
}