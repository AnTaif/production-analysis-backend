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
}