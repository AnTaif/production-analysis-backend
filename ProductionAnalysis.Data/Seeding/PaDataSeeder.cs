using Core.Auth;
using Core.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;
using ProductionAnalysis.Data.Models.Forms;

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
        await SeedPaTypesAsync();
        await SeedAdditionalOperationsAsync();
        await SeedOperationsAsync();
        await SeedProductsAsync();
        await SeedShiftsAsync();

        await SeedFormsAsync();

        await dbContext.SaveChangesAsync();
    }

    #region Users

    private async Task SeedRolesAsync()
    {
        var roles = RolesConstants.GetRoles();

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
            (@operator, [RolesConstants.Operator]),
            (analyst, [RolesConstants.Analyst]),
            (admin, [RolesConstants.Admin]),
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

    private Task SeedEmployeesAsync()
    {
        if (dbContext.Employees.Any())
            return Task.CompletedTask;

        dbContext.Employees.AddRange(
            new EmployeeDbo
            {
                Id = 1,
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                Position = "Бригадир",
                DepartmentId = 1
            },
            new EmployeeDbo
            {
                Id = 2,
                FirstName = "Петр",
                LastName = "Петров",
                MiddleName = "Петрович",
                Position = "Кладовщик",
                DepartmentId = 1
            },
            new EmployeeDbo
            {
                Id = 3,
                FirstName = "Алексей",
                LastName = "Сидоров",
                MiddleName = "Алексеевич",
                Position = "Мастер",
                DepartmentId = 2
            }
        );

        return Task.CompletedTask;
    }

    #endregion

    #region PaTypes

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

    #endregion

    #region Operations

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
            new ShiftDbo { Id = 1, Name = "1", StartTime = new TimeOnly(7, 0) },
            new ShiftDbo { Id = 2, Name = "2", StartTime = new TimeOnly(16, 0) },
            new ShiftDbo { Id = 3, Name = "3 (ночная)", StartTime = new TimeOnly(0, 0) }
        );

        return Task.CompletedTask;
    }

    #endregion

    #region Forms

    private Task SeedFormsAsync()
    {
        if (dbContext.Forms.Any())
            return Task.CompletedTask;

        var now = DateTime.UtcNow;

        dbContext.Forms.AddRange(
            new FormDbo
            {
                Id = 0, // Будет сгенерировано автоматически
                PaTypeId = 1,
                Status = 0,
                Context = "{\"shift\": 1, \"department\": 1}",
                TemplateSnapshot =
                    "{\"tableColumns\": [{\"id\": 1, \"name\": \"value\", \"inputType\": 2, \"valueType\": 3}]}",
                CreationDate = now,
                UpdateDate = now,
                CreatorId = dbContext.Users.First().Id,
                LastEditorId = dbContext.Users.First().Id
            },
            new FormDbo
            {
                Id = 0, // Будет сгенерировано автоматически
                PaTypeId = 2,
                Status = 1,
                Context = "{\"shift\": 1, \"department\": 1}",
                TemplateSnapshot =
                    "{\"tableColumns\": [{\"id\": 1, \"name\": \"value\", \"inputType\": 2, \"valueType\": 3}]}",
                CreationDate = now,
                UpdateDate = now,
                CreatorId = dbContext.Users.First().Id,
                LastEditorId = dbContext.Users.First().Id
            }
        );

        return Task.CompletedTask;
    }

    #endregion
}