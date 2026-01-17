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
            },
            // Операция "Установка прибора" с под-операциями
            new OperationDbo
            {
                Id = 7,
                Name = "Установка прибора",
                DurationInSeconds = 2100, // 35 мин (общий цикл)
                BasedOnType = 1,
                BasedOperationId = null,
                BasedProductId = null
            },
            // Под-операции для "Установка прибора"
            new OperationDbo
            {
                Id = 8,
                Name = "Подсборка",
                DurationInSeconds = 900, // 15 мин
                BasedOnType = 2,
                BasedOperationId = 7,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 9,
                Name = "Установка",
                DurationInSeconds = 600, // 10 мин
                BasedOnType = 2,
                BasedOperationId = 7,
                BasedProductId = null
            },
            new OperationDbo
            {
                Id = 10,
                Name = "Настройка",
                DurationInSeconds = 600, // 10 мин
                BasedOnType = 2,
                BasedOperationId = 7,
                BasedProductId = null
            },
            // Операции для сборки (могут быть связаны с продуктом)
            new OperationDbo
            {
                Id = 11,
                Name = "Установка рамы",
                DurationInSeconds = 3300, // 55 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 12,
                Name = "Установка гидросистемы",
                DurationInSeconds = 2100, // 35 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 13,
                Name = "Установка двигателя",
                DurationInSeconds = 1800, // 30 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 14,
                Name = "Установка переднего моста",
                DurationInSeconds = 1800, // 30 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 15,
                Name = "Установка кабины",
                DurationInSeconds = 2400, // 40 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 16,
                Name = "Подключение аппаратуры",
                DurationInSeconds = 2400, // 40 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 17,
                Name = "Соединение шарнира",
                DurationInSeconds = 2700, // 45 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 18,
                Name = "Установка гидроцилиндров",
                DurationInSeconds = 5100, // 85 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 19,
                Name = "Разводка электрики",
                DurationInSeconds = 3000, // 50 мин
                BasedOnType = 3,
                BasedOperationId = null,
                BasedProductId = 1
            },
            new OperationDbo
            {
                Id = 20,
                Name = "Подключение электрики",
                DurationInSeconds = 2400, // 40 мин
                BasedOnType = 3,
                BasedOperationId = null,
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

    #endregion

    #region Templates

    private async Task SeedTemplatesAsync()
    {
        if (await dbContext.Templates.AnyAsync())
            return;

        var indicators = await dbContext.Indicators.ToDictionaryAsync(i => i.Id);

        // Базовые индикаторы
        var worktime = indicators[1];
        var plan = indicators[2];
        var fact = indicators[3];
        var deviation = indicators[4];
        var downtime = indicators[5];
        var downtimeResponsible = indicators[6];
        var downTimeReasonsGroup = indicators[7];
        var downtimeReasonAndActionsTaken = indicators[8];
        var operationName = indicators[10];
        var operationTime = indicators[11];
        var startTimePlan = indicators[12];
        var startTimeFact = indicators[13];
        var endTimePlan = indicators[14];
        var endTimeFact = indicators[15];
        var planMinutes = indicators[16];
        var factMinutes = indicators[17];
        var deviationMinutes = indicators[18];

        // Накопительные индикаторы
        var planCumulative = indicators[19];
        var factCumulative = indicators[20];
        var deviationCumulative = indicators[21];
        var deviationMinutesCumulative = indicators[22];

        var template1 = CreateTemplate(
            1,
            "По времени такта",
            PaType.SingleProductWithCycleTime
        );

        AddIndicatorsToTemplate(template1,
            [
                worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
                downtime, downtimeResponsible, downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        var template2 = CreateTemplate(
            2,
            "По мощности рабочего места",
            PaType.SingleProductWithWorkstationCapacity
        );

        AddIndicatorsToTemplate(template2,
            [
                worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
                downtime, downtimeResponsible, downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        var template3 = CreateTemplate(
            3,
            "Несколько номенклатур",
            PaType.MultipleProductsWithCycleTime
        );

        AddIndicatorsToTemplate(template3,
            [
                worktime, plan, planCumulative, fact, factCumulative, deviation, deviationCumulative,
                downtime, downtimeResponsible, downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        var template4 = CreateTemplate(
            4,
            "Менее 1 изделия в час",
            PaType.LessThanOnePerHour
        );

        AddIndicatorsToTemplate(template4,
            [
                worktime, operationName, operationTime, plan, planCumulative, fact, factCumulative,
                deviation, deviationCumulative, downtime, downtimeResponsible,
                downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        var template5 = CreateTemplate(
            5,
            "Менее 1 изделия в смену",
            PaType.LessThanOnePerShift
        );

        AddIndicatorsToTemplate(template5,
            [
                operationName, startTimePlan, startTimeFact, endTimePlan, endTimeFact, planMinutes, factMinutes,
                deviationMinutes, deviationMinutesCumulative, downtime, downtimeResponsible,
                downTimeReasonsGroup, downtimeReasonAndActionsTaken
            ]
        );

        dbContext.Templates.AddRange(template1, template2, template3, template4, template5);
        await dbContext.SaveChangesAsync();
    }

    private static TemplateDbo CreateTemplate(int id, string name, PaType paType, int version = 0)
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

    #endregion
}