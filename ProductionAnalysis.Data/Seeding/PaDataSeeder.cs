using Core.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;
using Shared.Constants;

namespace ProductionAnalysis.Data.Seeding;

public class PaDataSeeder(
    PaDbContext dbContext,
    UserManager<UserDbo> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ILogger<PaDataSeeder> logger
)
    : DataSeeder(dbContext, logger)
{
    protected override async Task<bool> ShouldSeedAsync() =>
        await dbContext.Database.EnsureCreatedAsync() || !dbContext.Users.Any();

    protected override async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
        await SeedEnterprisesAsync();
        await SeedDepartmentsAsync();
        await SeedDowntimeReasonGroupsAsync();
        await SeedEmployeesAsync();
        await SeedAuxiliaryOperationsAsync();
        await SeedOperationsAsync();
        await SeedProductsAsync();
        await SeedShiftsAsync();
        await SeedShiftSchedulesAsync();
        await SeedIndicatorsAsync();
        await SeedTemplatesAsync();

        await dbContext.SaveChangesAsync();
    }

    #region Users

    private async Task SeedRolesAsync()
    {
        var roles = Roles.GetRoles();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var identityRole = new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = role,
                    NormalizedName = role.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                await roleManager.CreateAsync(identityRole);
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        var @operator = new UserDbo
        {
            Id = Guid.NewGuid(),
            Email = "operator@mail.ru",
            UserName = "operator@mail.ru",
            FirstName = "Operator",
            LastName = "LastName",
            MiddleName = "MiddleName"
        };

        var departmentHead = new UserDbo
        {
            Id = Guid.NewGuid(),
            Email = "departmentHead@mail.ru",
            UserName = "departmentHead@mail.ru",
            FirstName = "departmentHead",
            LastName = "LastName",
            MiddleName = "MiddleName"
        };

        var analyst = new UserDbo
        {
            Id = Guid.NewGuid(),
            Email = "analyst@mail.ru",
            UserName = "analyst@mail.ru",
            FirstName = "Analyst",
            LastName = "LastName",
            MiddleName = "MiddleName"
        };

        var admin = new UserDbo
        {
            Id = Guid.NewGuid(),
            Email = "admin@mail.ru",
            UserName = "admin@mail.ru",
            FirstName = "Admin",
            LastName = "LastName",
            MiddleName = "MiddleName"
        };

        var users = new List<(UserDbo, string[])>
        {
            (@operator, [Roles.Operator]),
            (departmentHead, [Roles.DepartmentHead]),
            (analyst, [Roles.Analyst]),
            (admin, [Roles.Admin]),
        };

        await CreateUsersAsync(users);
    }

    private async Task CreateUsersAsync(IEnumerable<(UserDbo, string[])> users)
    {
        foreach (var (user, roles) in users)
        {
            var result = await userManager.CreateAsync(user, "password");

            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(user, roles);
            }
        }
    }

    #endregion

    #region Enterprises

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

    #endregion

    #region Departments

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

    #endregion

    #region DowntimeReasonGroups

    private Task SeedDowntimeReasonGroupsAsync()
    {
        if (dbContext.DowntimeReasonGroups.Any())
            return Task.CompletedTask;

        dbContext.DowntimeReasonGroups.AddRange(
            new DowntimeReasonGroupDbo
            {
                Id = 1,
                Name = "Орг.",
                Description = "Организационные причины (отсутствие или неопытность работника, опоздание и тд.)"
            },
            new DowntimeReasonGroupDbo
            {
                Id = 2,
                Name = "Тех.",
                Description = "Технические причины (поломка оборудования / инструмента, нет энергоносителей и тд.)"
            },
            new DowntimeReasonGroupDbo
            {
                Id = 3,
                Name = "Лог.",
                Description = "Логистика, нет поставок (заготовок, инструмента, расходных материалов)"
            },
            new DowntimeReasonGroupDbo
            {
                Id = 4,
                Name = "Рег.",
                Description = "Регламентные работы"
            },
            new DowntimeReasonGroupDbo
            {
                Id = 5,
                Name = "Кач.",
                Description = "Качество"
            },
            new DowntimeReasonGroupDbo
            {
                Id = 6,
                Name = "Восп.",
                Description = "Восполнение потерянных объемов"
            }
        );

        return Task.CompletedTask;
    }

    #endregion

    #region Employees

    private async Task SeedEmployeesAsync()
    {
        if (dbContext.Employees.Any())
            return;

        // Получаем пользователей для связи
        var operatorUser = await userManager.FindByEmailAsync("operator@mail.ru");
        var departmentHeadUser = await userManager.FindByEmailAsync("departmentHead@mail.ru");
        var analystUser = await userManager.FindByEmailAsync("analyst@mail.ru");
        var adminUser = await userManager.FindByEmailAsync("admin@mail.ru");

        dbContext.Employees.AddRange(
            new EmployeeDbo
            {
                Id = 1,
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                Position = "Бригадир",
                DepartmentId = 1,
                UserId = operatorUser?.Id
            },
            new EmployeeDbo
            {
                Id = 2,
                FirstName = "Петр",
                LastName = "Петров",
                MiddleName = "Петрович",
                Position = "Кладовщик",
                DepartmentId = 1,
                UserId = departmentHeadUser?.Id
            },
            new EmployeeDbo
            {
                Id = 3,
                FirstName = "Алексей",
                LastName = "Сидоров",
                MiddleName = "Алексеевич",
                Position = "Мастер",
                DepartmentId = 2,
                UserId = analystUser?.Id
            },
            new EmployeeDbo
            {
                Id = 4,
                FirstName = "Admin",
                LastName = "LastName",
                MiddleName = "MiddleName",
                Position = "Администратор",
                DepartmentId = 1,
                UserId = adminUser?.Id
            }
        );
    }

    #endregion


    #region Operations

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

    #endregion

    #region Products

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

    #endregion

    #region Shifts

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

    #endregion

    #region ShiftSchedules

    private Task SeedShiftSchedulesAsync()
    {
        if (dbContext.ShiftSchedules.Any())
            return Task.CompletedTask;

        dbContext.ShiftSchedules.AddRange(
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
            }
        );

        return Task.CompletedTask;
    }

    #endregion

    #region Indicators

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

    #endregion

    #region Templates

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
        template1.Indicators.Add(worktime);
        template1.Indicators.Add(plan);
        template1.Indicators.Add(fact);
        template1.Indicators.Add(deviation);
        template1.Indicators.Add(downtime);
        template1.Indicators.Add(downtimeResponsible);
        template1.Indicators.Add(downtimeReason);
        template1.Indicators.Add(downTimeReasonsGroup);
        template1.Indicators.Add(actionsTaken);

        dbContext.Templates.Add(template1);

        // Шаблон для типа 2
        var template2 = new TemplateDbo
        {
            Id = 2,
            Name = "Почасовой по мощности рабочего места",
            PaTypeId = (int)PaType.SingleProductWithWorkstationCapacity,
            Version = 0,
        };

        template2.Indicators.Add(worktime);
        template2.Indicators.Add(plan);
        template2.Indicators.Add(fact);
        template2.Indicators.Add(deviation);
        template2.Indicators.Add(downtime);
        template2.Indicators.Add(downtimeResponsible);
        template2.Indicators.Add(downTimeReasonsGroup);
        template2.Indicators.Add(downtimeReason);
        template2.Indicators.Add(actionsTaken);

        dbContext.Templates.Add(template2);

        // Шаблон для типа 3
        var template3 = new TemplateDbo
        {
            Id = 3,
            Name = "Почасовой по мощности рабочего места",
            PaTypeId = (int)PaType.MultipleProductsWithCycleTime,
            Version = 0,
        };

        template3.Indicators.Add(worktime);
        template3.Indicators.Add(plan);
        template3.Indicators.Add(fact);
        template3.Indicators.Add(deviation);
        template3.Indicators.Add(downtime);
        template3.Indicators.Add(downtimeResponsible);
        template3.Indicators.Add(downTimeReasonsGroup);
        template3.Indicators.Add(downtimeReason);
        template3.Indicators.Add(actionsTaken);

        dbContext.Templates.Add(template3);

        // Шаблон для типа 4
        var template4 = new TemplateDbo
        {
            Id = 4,
            Name = "Менее 1 изделия в час",
            PaTypeId = (int)PaType.LessThanOnePerHour,
            Version = 0
        };
        template4.Indicators.Add(worktime);
        template4.Indicators.Add(plan);
        template4.Indicators.Add(operationName);
        template4.Indicators.Add(operationTime);
        template4.Indicators.Add(fact);
        template4.Indicators.Add(deviation);
        template4.Indicators.Add(downtime);
        template4.Indicators.Add(downtimeResponsible);
        template4.Indicators.Add(downtimeReason);
        template4.Indicators.Add(downTimeReasonsGroup);
        template4.Indicators.Add(actionsTaken);

        dbContext.Templates.Add(template4);

        // Шаблон для типа 5
        var template5 = new TemplateDbo
        {
            Id = 5,
            Name = "Менее 1 изделия в смену",
            PaTypeId = (int)PaType.LessThanOnePerShift,
            Version = 0
        };
        template5.Indicators.Add(operationName);
        template5.Indicators.Add(startTimePlan);
        template5.Indicators.Add(startTimeFact);
        template5.Indicators.Add(endTimePlan);
        template5.Indicators.Add(endTimeFact);
        template5.Indicators.Add(planMinutes);
        template5.Indicators.Add(factMinutes);
        template5.Indicators.Add(deviationMinutes);
        template5.Indicators.Add(downtime);
        template5.Indicators.Add(downtimeResponsible);
        template5.Indicators.Add(downtimeReason);
        template5.Indicators.Add(downTimeReasonsGroup);
        template5.Indicators.Add(actionsTaken);

        dbContext.Templates.Add(template5);
        await dbContext.SaveChangesAsync();
    }

    #endregion
}