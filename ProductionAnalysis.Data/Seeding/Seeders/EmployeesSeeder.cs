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
                Id = 1,
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                Position = "Бригадир",
                Email = "operator@mail.ru",
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
                Email = "departmentHead@mail.ru",
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
                Email = "analyst@mail.ru",
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
                Email = "admin@mail.ru",
                DepartmentId = 1,
                UserId = adminUser?.Id
            },
            new EmployeeDbo
            {
                Id = 5,
                FirstName = "Сергей",
                LastName = "Кузнецов",
                MiddleName = "Сергеевич",
                Position = "Оператор",
                Email = "kuznetsov@mail.ru",
                DepartmentId = 1,
                UserId = null
            },
            new EmployeeDbo
            {
                Id = 6,
                FirstName = "Дмитрий",
                LastName = "Смирнов",
                MiddleName = "Дмитриевич",
                Position = "Старший оператор",
                Email = "smirnov@mail.ru",
                DepartmentId = 1,
                UserId = null
            },
            new EmployeeDbo
            {
                Id = 7,
                FirstName = "Андрей",
                LastName = "Попов",
                MiddleName = "Андреевич",
                Position = "Технолог",
                Email = "popov@mail.ru",
                DepartmentId = 2,
                UserId = null
            },
            new EmployeeDbo
            {
                Id = 8,
                FirstName = "Михаил",
                LastName = "Соколов",
                MiddleName = "Михайлович",
                Position = "Инженер",
                Email = "sokolov@mail.ru",
                DepartmentId = 2,
                UserId = null
            },
            new EmployeeDbo
            {
                Id = 9,
                FirstName = "Елена",
                LastName = "Волкова",
                MiddleName = "Владимировна",
                Position = "Контролер качества",
                Email = "volkova@mail.ru",
                DepartmentId = 2,
                UserId = null
            },
            new EmployeeDbo
            {
                Id = 10,
                FirstName = "Николай",
                LastName = "Лебедев",
                MiddleName = "Николаевич",
                Position = "Наладчик",
                Email = "lebedev@mail.ru",
                DepartmentId = 1,
                UserId = null
            },
            new EmployeeDbo
            {
                Id = 11,
                FirstName = "Владимир",
                LastName = "Новиков",
                MiddleName = "Владимирович",
                Position = "Сварщик",
                Email = "novikov@mail.ru",
                DepartmentId = 3,
                UserId = null
            },
            new EmployeeDbo
            {
                Id = 12,
                FirstName = "Олег",
                LastName = "Морозов",
                MiddleName = "Олегович",
                Position = "Токарь",
                Email = "morozov@mail.ru",
                DepartmentId = 3,
                UserId = null
            }
        );
    }
}