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
                IsCumulative = false,
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
                IsCumulative = true,
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
                IsCumulative = true,
                HasSummation = true
            },
            new IndicatorDbo
            {
                Id = 4,
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
                Id = 5,
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
                Id = 6,
                Name = "Ответственный за простой",
                ValueType = FieldValueTypes.Text,
                InputType = FieldInputTypes.Dictionary,
                ValueSelector = "employees",
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
                Name = "Причины отклонения/комментарий",
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
                Id = 10,
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
                Id = 11,
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
                Id = 12,
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
                Id = 13,
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
                Id = 14,
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
                Id = 15,
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
                Id = 17,
                Name = "Факт, мин.",
                ValueType = FieldValueTypes.Number,
                InputType = FieldInputTypes.Manual,
                ValueSelector = null,
                Formula = null,
                IsCumulative = true,
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

        var worktime = await dbContext.Indicators.FirstAsync(i => i.Id == 1);
        var plan = await dbContext.Indicators.FirstAsync(i => i.Id == 2);
        var fact = await dbContext.Indicators.FirstAsync(i => i.Id == 3);
        var deviation = await dbContext.Indicators.FirstAsync(i => i.Id == 4);
        var downtime = await dbContext.Indicators.FirstAsync(i => i.Id == 5);
        var downtimeResponsible = await dbContext.Indicators.FirstAsync(i => i.Id == 6);
        var downTimeReasonsGroup = await dbContext.Indicators.FirstAsync(i => i.Id == 7);
        var downtimeReason = await dbContext.Indicators.FirstAsync(i => i.Id == 8);
        var actionsTaken = await dbContext.Indicators.FirstAsync(i => i.Id == 9);
        var operationName = await dbContext.Indicators.FirstAsync(i => i.Id == 10);
        var operationTime = await dbContext.Indicators.FirstAsync(i => i.Id == 11);
        var startTimePlan = await dbContext.Indicators.FirstAsync(i => i.Id == 12);
        var startTimeFact = await dbContext.Indicators.FirstAsync(i => i.Id == 13);
        var endTimePlan = await dbContext.Indicators.FirstAsync(i => i.Id == 14);
        var endTimeFact = await dbContext.Indicators.FirstAsync(i => i.Id == 15);
        var planMinutes = await dbContext.Indicators.FirstAsync(i => i.Id == 16);
        var factMinutes = await dbContext.Indicators.FirstAsync(i => i.Id == 17);
        var deviationMinutes = await dbContext.Indicators.FirstAsync(i => i.Id == 18);

        var template1 = new TemplateDbo
        {
            Id = 1,
            Name = "Почасовой по времени такта",
            PaTypeId = (int)PaType.SingleProductWithCycleTime,
            Version = 1
        };
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template1.Id, IndicatorId = worktime.Id, Indicator = worktime, Order = 0 });
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template1.Id, IndicatorId = plan.Id, Indicator = plan, Order = 1 });
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template1.Id, IndicatorId = fact.Id, Indicator = fact, Order = 2 });
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template1.Id, IndicatorId = deviation.Id, Indicator = deviation, Order = 3 });
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template1.Id, IndicatorId = downtime.Id, Indicator = downtime, Order = 4 });
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template1.Id, IndicatorId = downtimeResponsible.Id, Indicator = downtimeResponsible, Order = 5
        });
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template1.Id, IndicatorId = downtimeReason.Id, Indicator = downtimeReason, Order = 6 });
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template1.Id, IndicatorId = downTimeReasonsGroup.Id, Indicator = downTimeReasonsGroup,
            Order = 7
        });
        template1.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template1.Id, IndicatorId = actionsTaken.Id, Indicator = actionsTaken, Order = 8 });

        dbContext.Templates.Add(template1);

        // Шаблон для типа 2
        var template2 = new TemplateDbo
        {
            Id = 2,
            Name = "Почасовой по мощности рабочего места",
            PaTypeId = (int)PaType.SingleProductWithWorkstationCapacity,
            Version = 0,
        };

        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template2.Id, IndicatorId = worktime.Id, Indicator = worktime, Order = 0 });
        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template2.Id, IndicatorId = plan.Id, Indicator = plan, Order = 1 });
        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template2.Id, IndicatorId = fact.Id, Indicator = fact, Order = 2 });
        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template2.Id, IndicatorId = deviation.Id, Indicator = deviation, Order = 3 });
        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template2.Id, IndicatorId = downtime.Id, Indicator = downtime, Order = 4 });
        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template2.Id, IndicatorId = downtimeResponsible.Id, Indicator = downtimeResponsible, Order = 5
        });
        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template2.Id, IndicatorId = downTimeReasonsGroup.Id, Indicator = downTimeReasonsGroup,
            Order = 6
        });
        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template2.Id, IndicatorId = downtimeReason.Id, Indicator = downtimeReason, Order = 7 });
        template2.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template2.Id, IndicatorId = actionsTaken.Id, Indicator = actionsTaken, Order = 8 });

        dbContext.Templates.Add(template2);

        // Шаблон для типа 3
        var template3 = new TemplateDbo
        {
            Id = 3,
            Name = "Почасовой по мощности рабочего места",
            PaTypeId = (int)PaType.MultipleProductsWithCycleTime,
            Version = 0,
        };

        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template3.Id, IndicatorId = worktime.Id, Indicator = worktime, Order = 0 });
        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template3.Id, IndicatorId = plan.Id, Indicator = plan, Order = 1 });
        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template3.Id, IndicatorId = fact.Id, Indicator = fact, Order = 2 });
        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template3.Id, IndicatorId = deviation.Id, Indicator = deviation, Order = 3 });
        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template3.Id, IndicatorId = downtime.Id, Indicator = downtime, Order = 4 });
        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template3.Id, IndicatorId = downtimeResponsible.Id, Indicator = downtimeResponsible, Order = 5
        });
        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template3.Id, IndicatorId = downTimeReasonsGroup.Id, Indicator = downTimeReasonsGroup,
            Order = 6
        });
        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template3.Id, IndicatorId = downtimeReason.Id, Indicator = downtimeReason, Order = 7 });
        template3.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template3.Id, IndicatorId = actionsTaken.Id, Indicator = actionsTaken, Order = 8 });

        dbContext.Templates.Add(template3);

        // Шаблон для типа 4
        var template4 = new TemplateDbo
        {
            Id = 4,
            Name = "Менее 1 изделия в час",
            PaTypeId = (int)PaType.LessThanOnePerHour,
            Version = 0
        };
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = worktime.Id, Indicator = worktime, Order = 0 });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = plan.Id, Indicator = plan, Order = 1 });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = operationName.Id, Indicator = operationName, Order = 2 });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = operationTime.Id, Indicator = operationTime, Order = 3 });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = fact.Id, Indicator = fact, Order = 4 });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = deviation.Id, Indicator = deviation, Order = 5 });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = downtime.Id, Indicator = downtime, Order = 6 });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template4.Id, IndicatorId = downtimeResponsible.Id, Indicator = downtimeResponsible, Order = 7
        });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = downtimeReason.Id, Indicator = downtimeReason, Order = 8 });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template4.Id, IndicatorId = downTimeReasonsGroup.Id, Indicator = downTimeReasonsGroup,
            Order = 9
        });
        template4.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template4.Id, IndicatorId = actionsTaken.Id, Indicator = actionsTaken, Order = 10 });

        dbContext.Templates.Add(template4);

        // Шаблон для типа 5
        var template5 = new TemplateDbo
        {
            Id = 5,
            Name = "Менее 1 изделия в смену",
            PaTypeId = (int)PaType.LessThanOnePerShift,
            Version = 0
        };
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = operationName.Id, Indicator = operationName, Order = 0 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = startTimePlan.Id, Indicator = startTimePlan, Order = 1 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = startTimeFact.Id, Indicator = startTimeFact, Order = 2 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = endTimePlan.Id, Indicator = endTimePlan, Order = 3 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = endTimeFact.Id, Indicator = endTimeFact, Order = 4 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = planMinutes.Id, Indicator = planMinutes, Order = 5 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = factMinutes.Id, Indicator = factMinutes, Order = 6 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = deviationMinutes.Id, Indicator = deviationMinutes, Order = 7 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = downtime.Id, Indicator = downtime, Order = 8 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template5.Id, IndicatorId = downtimeResponsible.Id, Indicator = downtimeResponsible, Order = 9
        });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = downtimeReason.Id, Indicator = downtimeReason, Order = 10 });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
        {
            TemplateId = template5.Id, IndicatorId = downTimeReasonsGroup.Id, Indicator = downTimeReasonsGroup,
            Order = 11
        });
        template5.TemplateIndicators.Add(new TemplateIndicatorDbo
            { TemplateId = template5.Id, IndicatorId = actionsTaken.Id, Indicator = actionsTaken, Order = 12 });

        dbContext.Templates.Add(template5);
        dbContext.Templates.Add(template5);
        await dbContext.SaveChangesAsync();
    }
}