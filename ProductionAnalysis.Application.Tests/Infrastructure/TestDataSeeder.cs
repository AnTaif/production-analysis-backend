using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Domain;
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
                Name = "План, шт.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = "",
                Formula = null,
                IsCumulative = true,
                HasSummation = true,
            },
            new IndicatorDbo
            {
                Id = 2,
                Name = "Факт, шт.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = "",
                Formula = null,
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 3,
                Name = "Отклонение, шт.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Formula,
                ValueSelector = "",
                Formula = "indicator_2 - indicator_1",
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 4,
                Name = "Простой, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 5,
                Name = "Ответственный за простой",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                IsCumulative = false,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 6,
                Name = "Причины простоя",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                IsCumulative = false,
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
                IsCumulative = false,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 8,
                Name = "Принятые меры",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                IsCumulative = false,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 9,
                Name = "Наименование операции",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Dictionary,
                ValueSelector = null,
                Formula = null,
                IsCumulative = false,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 10,
                Name = "Время операции/элемента, мин.",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Context,
                ValueSelector = null,
                Formula = null,
                IsCumulative = false,
                HasSummation = false
            },
            new IndicatorDbo
            {
                Id = 11,
                Name = "Время начала план, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 12,
                Name = "Время начала факт, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 13,
                Name = "Время окончания план, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 14,
                Name = "Время окончания факт, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 16,
                Name = "Время работы, час.",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                IsCumulative = false,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 17,
                Name = "План, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 18,
                Name = "Факт, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                IsCumulative = true,
                HasSummation = true
            }
        );

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedTemplatesAsync()
    {
        if (await dbContext.Templates.AnyAsync())
            return;

        var worktime = await dbContext.Indicators.FirstAsync(i => i.Id == 16);
        var plan = await dbContext.Indicators.FirstAsync(i => i.Id == 1);
        var fact = await dbContext.Indicators.FirstAsync(i => i.Id == 2);
        var deviation = await dbContext.Indicators.FirstAsync(i => i.Id == 3);
        var downtime = await dbContext.Indicators.FirstAsync(i => i.Id == 4);
        var downtimeResponsible = await dbContext.Indicators.FirstAsync(i => i.Id == 5);
        var downtimeReason = await dbContext.Indicators.FirstAsync(i => i.Id == 6);
        var downTimeReasonsGroup = await dbContext.Indicators.FirstAsync(i => i.Id == 7);
        var actionsTaken = await dbContext.Indicators.FirstAsync(i => i.Id == 8);
        var operationName = await dbContext.Indicators.FirstAsync(i => i.Id == 9);
        var operationTime = await dbContext.Indicators.FirstAsync(i => i.Id == 10);
        var startTimePlan = await dbContext.Indicators.FirstAsync(i => i.Id == 11);
        var startTimeFact = await dbContext.Indicators.FirstAsync(i => i.Id == 12);
        var endTimePlan = await dbContext.Indicators.FirstAsync(i => i.Id == 13);
        var endTimeFact = await dbContext.Indicators.FirstAsync(i => i.Id == 14);
        var planMinutes = await dbContext.Indicators.FirstAsync(i => i.Id == 17);
        var factMinutes = await dbContext.Indicators.FirstAsync(i => i.Id == 18);

        var template1 = new TemplateDbo
        {
            Id = 1,
            Name = "Шаблон для изготовления продукции  более 1 шт. в час (по времени такта)",
            PaTypeId = 1,
            Version = 1
        };
        template1.Indicators.Add(worktime);
        template1.Indicators.Add(plan);
        template1.Indicators.Add(fact);
        template1.Indicators.Add(deviation);
        template1.Indicators.Add(downtime);

        dbContext.Templates.Add(template1);

        // Шаблон для типа "Менее 1 шт. в час"
        var template4 = new TemplateDbo
        {
            Id = 4,
            Name = "Шаблон для изготовления продукции менее 1 шт. в час",
            PaTypeId = 4,
            Version = 1
        };
        template4.Indicators.Add(worktime);
        template4.Indicators.Add(plan);
        template4.Indicators.Add(operationName);
        template4.Indicators.Add(operationTime);
        template4.Indicators.Add(fact);
        template4.Indicators.Add(deviation);
        template4.Indicators.Add(downtime);

        dbContext.Templates.Add(template4);

        // Шаблон для типа "Менее 1 шт. в смену"
        var template5 = new TemplateDbo
        {
            Id = 5,
            Name = "Шаблон для изготовления продукции менее 1 шт. в смену",
            PaTypeId = 5,
            Version = 1
        };
        template5.Indicators.Add(operationName);
        template5.Indicators.Add(startTimePlan);
        template5.Indicators.Add(startTimeFact);
        template5.Indicators.Add(endTimePlan);
        template5.Indicators.Add(endTimeFact);
        template5.Indicators.Add(planMinutes);
        template5.Indicators.Add(factMinutes);
        template5.Indicators.Add(deviation);
        template5.Indicators.Add(downtime);
        template5.Indicators.Add(downtimeResponsible);
        template5.Indicators.Add(downtimeReason);
        template5.Indicators.Add(downTimeReasonsGroup);
        template5.Indicators.Add(actionsTaken);

        dbContext.Templates.Add(template5);
        await dbContext.SaveChangesAsync();
    }
}