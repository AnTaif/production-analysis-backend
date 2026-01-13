using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Infrastructure;

public class TestDataSeeder
{
    private readonly PaDbContext dbContext;
    private readonly UserManager<UserDbo> userManager;
    private readonly RoleManager<IdentityRole<Guid>> roleManager;

    public TestDataSeeder(
        PaDbContext dbContext,
        UserManager<UserDbo> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.roleManager = roleManager;
    }

    public async Task SeedAllAsync()
    {
        await SeedEnterprisesAsync();
        await SeedDepartmentsAsync();
        await SeedShiftsAsync();
        await SeedAuxiliaryOperationsAsync();
        await SeedProductsAsync(); // Продукты должны быть созданы до операций
        await SeedOperationsAsync();
        await SeedIndicatorsAsync();
        await SeedTemplatesAsync();
        await dbContext.SaveChangesAsync();
    }

    private Task SeedEnterprisesAsync()
    {
        if (dbContext.Enterprises.Any())
            return Task.CompletedTask;

        dbContext.Enterprises.AddRange(
            new EnterpriseDbo { Id = 1, Name = "Предприятие №1" },
            new EnterpriseDbo { Id = 2, Name = "Завод в свердловской области" }
        );

        return Task.CompletedTask;
    }

    private Task SeedDepartmentsAsync()
    {
        if (dbContext.Departments.Any())
            return Task.CompletedTask;

        dbContext.Departments.AddRange(
            new DepartmentDbo { Id = 1, Name = "Цех №1", EnterpriseId = 1 },
            new DepartmentDbo { Id = 2, Name = "Цех №2", EnterpriseId = 1 },
            new DepartmentDbo { Id = 3, Name = "Литейный участок", EnterpriseId = 2 }
        );

        return Task.CompletedTask;
    }

    private Task SeedShiftsAsync()
    {
        if (dbContext.Shifts.Any())
            return Task.CompletedTask;

        dbContext.Shifts.AddRange(
            new ShiftDbo { Id = 1, Name = "1", StartTime = new TimeOnly(8, 0) },
            new ShiftDbo { Id = 2, Name = "2", StartTime = new TimeOnly(16, 0) },
            new ShiftDbo { Id = 3, Name = "3 (ночная)", StartTime = new TimeOnly(0, 0) }
        );

        return Task.CompletedTask;
    }

    private Task SeedAuxiliaryOperationsAsync()
    {
        if (dbContext.AuxiliaryOperations.Any())
            return Task.CompletedTask;

        dbContext.AuxiliaryOperations.AddRange(
            new AuxiliaryOperationDbo
            {
                Id = 1,
                Name = "Обед 30 мин",
                DurationInSeconds = 1800
            },
            new AuxiliaryOperationDbo
            {
                Id = 2,
                Name = "Перерыв 15 мин",
                DurationInSeconds = 900
            },
            new AuxiliaryOperationDbo
            {
                Id = 3,
                Name = "Уборка 15 мин",
                DurationInSeconds = 900
            },
            new AuxiliaryOperationDbo
            {
                Id = 4,
                Name = "Переналадка 15 мин",
                DurationInSeconds = 900
            }
        );

        return Task.CompletedTask;
    }

    private Task SeedOperationsAsync()
    {
        if (dbContext.Operations.Any())
            return Task.CompletedTask;

        dbContext.Operations.AddRange(
            new OperationDbo
            {
                Id = 1,
                Name = "Подготовка",
                DurationInSeconds = 300,
                BasedOnType = 1,
                BasedOperationId = null,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 2,
                Name = "Обработка",
                DurationInSeconds = 900,
                BasedOnType = 2,
                BasedOperationId = 1,
            },
            new OperationDbo
            {
                Id = 3,
                Name = "Сборка",
                DurationInSeconds = 1200,
                BasedOnType = 3,
                BasedProductId = 1
            },
            // Операции для продукта "Корпус редуктора" (Id = 1)
            new OperationDbo
            {
                Id = 4,
                Name = "Подсборка",
                DurationInSeconds = 900, // 15 мин
                BasedOnType = 3,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 5,
                Name = "Установка",
                DurationInSeconds = 600, // 10 мин
                BasedOnType = 3,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 6,
                Name = "Настройка",
                DurationInSeconds = 600, // 10 мин
                BasedOnType = 3,
                BasedProductId = 1
            }
        );

        return Task.CompletedTask;
    }

    private Task SeedProductsAsync()
    {
        if (dbContext.Products.Any())
            return Task.CompletedTask;

        dbContext.Products.AddRange(
            new ProductDbo
            {
                Id = 1,
                Name = "Корпус редуктора",
                TactTimeInSeconds = 600,
                EnterpriseId = 1
            },
            new ProductDbo
            {
                Id = 2,
                Name = "Вал привода",
                TactTimeInSeconds = 450,
                EnterpriseId = 1
            }
        );

        return Task.CompletedTask;
    }

    private async Task SeedIndicatorsAsync()
    {
        if (dbContext.Indicators.Any())
            return;

        dbContext.Indicators.AddRange(
            new IndicatorDbo
            {
                Id = 1,
                Name = "Время работы, час.",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 2,
                Name = "План, шт.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = "",
                Formula = null,
                HasSummation = true,
            },
            new IndicatorDbo
            {
                Id = 3,
                Name = "Факт, шт.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = "",
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 4,
                Name = "Отклонение, шт.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Formula,
                ValueSelector = "",
                Formula = "indicator_3 - indicator_2",
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 5,
                Name = "Простой, мин.",
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
                Name = "Время операции/элемента, мин.",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Context,
                ValueSelector = null,
                Formula = null,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 12,
                Name = "Время начала план, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 13,
                Name = "Время начала факт, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 14,
                Name = "Время окончания план, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 15,
                Name = "Время окончания факт, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 16,
                Name = "План, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 17,
                Name = "Факт, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 18,
                Name = "Отклонение, мин.",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Formula,
                ValueSelector = null,
                Formula = "indicator_17 - indicator_16",
                HasSummation = true
            },
            // Накопительные индикаторы
            new IndicatorDbo
            {
                Id = 19,
                Name = "План, шт. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "2", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 20,
                Name = "Факт, шт. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "3", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 21,
                Name = "Отклонение, шт. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "4", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 22,
                Name = "Простой, мин. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "5", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 23,
                Name = "Время начала план, мин. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "12", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 24,
                Name = "Время начала факт, мин. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "13", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 25,
                Name = "Время окончания план, мин. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "14", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 26,
                Name = "Время окончания факт, мин. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "15", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 27,
                Name = "План, мин. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "16", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 28,
                Name = "Факт, мин. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "17", // ID базового индикатора
                Formula = null,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 29,
                Name = "Отклонение, мин. (накопительно)",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Cumulative,
                ValueSelector = "18", // ID базового индикатора
                Formula = null,
                HasSummation = true
            }
        );

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedTemplatesAsync()
    {
        if (await dbContext.Templates.AnyAsync())
            return;

        // Загружаем все индикаторы одним запросом
        var indicators = await dbContext.Indicators.ToDictionaryAsync(i => i.Id);
        var indicatorsById = (int id) => indicators[id];

        // Базовые индикаторы
        var worktime = indicatorsById(1);
        var plan = indicatorsById(2);
        var fact = indicatorsById(3);
        var deviation = indicatorsById(4);
        var downtime = indicatorsById(5);
        var downtimeResponsible = indicatorsById(6);
        var downTimeReasonsGroup = indicatorsById(7);
        var downtimeReason = indicatorsById(8);
        var operationName = indicatorsById(10);
        var operationTime = indicatorsById(11);
        var startTimePlan = indicatorsById(12);
        var startTimeFact = indicatorsById(13);
        var endTimePlan = indicatorsById(14);
        var endTimeFact = indicatorsById(15);
        var planMinutes = indicatorsById(16);
        var factMinutes = indicatorsById(17);
        var deviationMinutes = indicatorsById(18);

        // Накопительные индикаторы
        var planCumulative = indicatorsById(19);
        var factCumulative = indicatorsById(20);
        var deviationCumulative = indicatorsById(21);
        var downtimeCumulative = indicatorsById(22);
        var startTimePlanCumulative = indicatorsById(23);
        var startTimeFactCumulative = indicatorsById(24);
        var endTimePlanCumulative = indicatorsById(25);
        var endTimeFactCumulative = indicatorsById(26);
        var planMinutesCumulative = indicatorsById(27);
        var factMinutesCumulative = indicatorsById(28);
        var deviationMinutesCumulative = indicatorsById(29);

        // Шаблон 1: Почасовой по времени такта
        var template1 = CreateTemplate(1, "Почасовой по времени такта", PaType.SingleProductWithCycleTime, 1);
        AddIndicatorsToTemplate(template1,
        [
            worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
            downtime, downtimeCumulative, downtimeResponsible, downtimeReason, downTimeReasonsGroup
        ]);

        // Шаблон 2: Почасовой по мощности рабочего места
        var template2 = CreateTemplate(2, "Почасовой по мощности рабочего места",
            PaType.SingleProductWithWorkstationCapacity, 0);
        AddIndicatorsToTemplate(template2,
        [
            worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
            downtime, downtimeCumulative, downtimeResponsible, downTimeReasonsGroup, downtimeReason
        ]);

        // Шаблон 3: Почасовой по мощности рабочего места (множественные продукты)
        var template3 = CreateTemplate(3, "Почасовой по мощности рабочего места", PaType.MultipleProductsWithCycleTime,
            0);
        AddIndicatorsToTemplate(template3,
        [
            worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
            downtime, downtimeCumulative, downtimeResponsible, downTimeReasonsGroup, downtimeReason
        ]);

        // Шаблон 4: Менее 1 изделия в час
        var template4 = CreateTemplate(4, "Менее 1 изделия в час", PaType.LessThanOnePerHour, 0);
        AddIndicatorsToTemplate(template4,
        [
            worktime, plan, planCumulative, operationName, operationTime, fact, factCumulative,
            deviation, deviationCumulative, downtime, downtimeCumulative, downtimeResponsible,
            downtimeReason, downTimeReasonsGroup
        ]);

        // Шаблон 5: Менее 1 изделия в смену
        var template5 = CreateTemplate(5, "Менее 1 изделия в смену", PaType.LessThanOnePerShift, 0);
        AddIndicatorsToTemplate(template5,
        [
            operationName, startTimePlan, startTimePlanCumulative, startTimeFact, startTimeFactCumulative,
            endTimePlan, endTimePlanCumulative, endTimeFact, endTimeFactCumulative,
            planMinutes, planMinutesCumulative, factMinutes, factMinutesCumulative,
            deviationMinutes, deviationMinutesCumulative, downtime, downtimeResponsible,
            downtimeReason, downTimeReasonsGroup
        ]);

        dbContext.Templates.AddRange(template1, template2, template3, template4, template5);
        await dbContext.SaveChangesAsync();
    }

    private static TemplateDbo CreateTemplate(int id, string name, PaType paType, int version)
    {
        return new TemplateDbo
        {
            Id = id,
            Name = name,
            PaTypeId = (int)paType,
            Version = version
        };
    }

    private static void AddIndicatorsToTemplate(
        TemplateDbo template,
        IndicatorDbo[] indicators)
    {
        for (short order = 0; order < indicators.Length; order++)
        {
            var indicator = indicators[order];
            template.TemplateIndicators.Add(new TemplateIndicatorDbo
            {
                TemplateId = template.Id,
                IndicatorId = indicator.Id,
                Indicator = indicator,
                Order = order
            });
        }
    }
}