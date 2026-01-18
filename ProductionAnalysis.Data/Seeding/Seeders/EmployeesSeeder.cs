using Microsoft.AspNetCore.Identity;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Seeding.Seeders;

public class EmployeesSeeder(PaDbContext dbContext, UserManager<UserDbo> userManager)
{
    public async Task SeedAsync()
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
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                PositionId = 1, // Оператор
                Email = "operator@mail.ru",
                DepartmentId = 1,
                UserId = operatorUser?.Id
            },
            new EmployeeDbo
            {
                FirstName = "Петр",
                LastName = "Петров",
                MiddleName = "Петрович",
                PositionId = 2, // Начальник участка
                Email = "departmentHead@mail.ru",
                DepartmentId = 1,
                UserId = departmentHeadUser?.Id
            },
            new EmployeeDbo
            {
                FirstName = "Алексей",
                LastName = "Сидоров",
                MiddleName = "Алексеевич",
                PositionId = 3, // Аналитик
                Email = "analyst@mail.ru",
                DepartmentId = 2,
                UserId = analystUser?.Id
            },
            new EmployeeDbo
            {
                FirstName = "Admin",
                LastName = "LastName",
                MiddleName = "MiddleName",
                PositionId = 4, // Администратор
                Email = "admin@mail.ru",
                DepartmentId = 1,
                UserId = adminUser?.Id
            },
            new EmployeeDbo
            {
                FirstName = "Сергей",
                LastName = "Кузнецов",
                MiddleName = "Сергеевич",
                PositionId = 1, // Оператор
                Email = "kuznetsov@mail.ru",
                DepartmentId = 1,
                UserId = null
            },
            new EmployeeDbo
            {
                FirstName = "Дмитрий",
                LastName = "Смирнов",
                MiddleName = "Дмитриевич",
                PositionId = 1, // Оператор (вместо несуществующего "Старший оператор")
                Email = "smirnov@mail.ru",
                DepartmentId = 1,
                UserId = null
            },
            new EmployeeDbo
            {
                FirstName = "Андрей",
                LastName = "Попов",
                MiddleName = "Андреевич",
                PositionId = 7, // Технолог
                Email = "popov@mail.ru",
                DepartmentId = 2,
                UserId = null
            },
            new EmployeeDbo
            {
                FirstName = "Михаил",
                LastName = "Соколов",
                MiddleName = "Михайлович",
                PositionId = 8, // Инженер
                Email = "sokolov@mail.ru",
                DepartmentId = 2,
                UserId = null
            },
            new EmployeeDbo
            {
                FirstName = "Елена",
                LastName = "Волкова",
                MiddleName = "Владимировна",
                PositionId = 8, // Инженер (вместо несуществующего "Контролер качества")
                Email = "volkova@mail.ru",
                DepartmentId = 2,
                UserId = null
            },
            new EmployeeDbo
            {
                FirstName = "Николай",
                LastName = "Лебедев",
                MiddleName = "Николаевич",
                PositionId = 10, // Наладчик
                Email = "lebedev@mail.ru",
                DepartmentId = 1,
                UserId = null
            },
            new EmployeeDbo
            {
                FirstName = "Владимир",
                LastName = "Новиков",
                MiddleName = "Владимирович",
                PositionId = 11, // Сварщик
                Email = "novikov@mail.ru",
                DepartmentId = 3,
                UserId = null
            },
            new EmployeeDbo
            {
                FirstName = "Олег",
                LastName = "Морозов",
                MiddleName = "Олегович",
                PositionId = 12, // Токарь
                Email = "morozov@mail.ru",
                DepartmentId = 3,
                UserId = null
            }
        );
    }
}