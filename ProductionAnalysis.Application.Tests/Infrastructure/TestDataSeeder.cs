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
        await SeedPaTypesAsync();
        await SeedShiftsAsync();
        await SeedAdditionalOperationsAsync();
        await SeedProductsAsync();
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

    private Task SeedPaTypesAsync()
    {
        if (dbContext.PaTypes.Any())
            return Task.CompletedTask;

        dbContext.PaTypes.AddRange(
            new PaTypeDbo { Id = 1, Name = "Более 1 шт. в час (по времени такта)" },
            new PaTypeDbo { Id = 2, Name = "Более 1 шт. в час исходя из мощности рабочего  места" },
            new PaTypeDbo { Id = 3, Name = "Более 1 шт. в час нескольких номенклатур" },
            new PaTypeDbo { Id = 4, Name = "Менее 1 шт. в час" },
            new PaTypeDbo { Id = 5, Name = "Менее 1 шт. в смену" }
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

    private Task SeedAdditionalOperationsAsync()
    {
        if (dbContext.AdditionalOperations.Any())
            return Task.CompletedTask;

        dbContext.AdditionalOperations.AddRange(
            new AdditionalOperationDbo
            {
                Id = 1,
                Name = "Обед 30 мин",
                DurationInSeconds = 1800
            },
            new AdditionalOperationDbo
            {
                Id = 2,
                Name = "Перерыв 15 мин",
                DurationInSeconds = 900
            },
            new AdditionalOperationDbo
            {
                Id = 3,
                Name = "Уборка 15 мин",
                DurationInSeconds = 900
            },
            new AdditionalOperationDbo
            {
                Id = 4,
                Name = "Переналадка 15 мин",
                DurationInSeconds = 900
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
                Id = 16,
                Name = "Время работы, час.",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Initialization,
                ValueSelector = null,
                Formula = null,
                IsCumulative = false,
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
        await dbContext.SaveChangesAsync();
    }
}