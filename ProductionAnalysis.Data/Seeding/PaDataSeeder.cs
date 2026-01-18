using Core.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProductionAnalysis.Application.Implementation.Forms;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Seeding.Seeders;

namespace ProductionAnalysis.Data.Seeding;

public class PaDataSeeder(
    PaDbContext dbContext,
    UserManager<UserDbo> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ILogger<PaDataSeeder> logger,
    IFormsService formsService
)
    : DataSeeder(dbContext, logger)
{
    protected override async Task<bool> ShouldSeedAsync() =>
        await dbContext.Database.EnsureCreatedAsync() || !dbContext.Users.Any();

    protected override async Task SeedAsync()
    {
        var rolesSeeder = new RolesSeeder(dbContext, roleManager);
        var usersSeeder = new UsersSeeder(dbContext, userManager);
        var enterprisesSeeder = new EnterprisesSeeder(dbContext);
        var departmentsSeeder = new DepartmentsSeeder(dbContext);
        var downtimeReasonGroupsSeeder = new DowntimeReasonGroupsSeeder(dbContext);
        var employeesSeeder = new EmployeesSeeder(dbContext, userManager);
        var auxiliaryOperationsSeeder = new AuxiliaryOperationsSeeder(dbContext);
        var operationsSeeder = new OperationsSeeder(dbContext);
        var productsSeeder = new ProductsSeeder(dbContext);
        var shiftsSeeder = new ShiftsSeeder(dbContext);
        var shiftSchedulesSeeder = new ShiftSchedulesSeeder(dbContext);
        var indicatorsSeeder = new IndicatorsSeeder(dbContext);
        var templatesSeeder = new TemplatesSeeder(dbContext);
        var formsSeeder = new FormsSeeder(dbContext, userManager, formsService, logger);

        await rolesSeeder.SeedAsync();
        await usersSeeder.SeedAsync();
        await enterprisesSeeder.SeedAsync();
        await departmentsSeeder.SeedAsync();
        await downtimeReasonGroupsSeeder.SeedAsync();
        await employeesSeeder.SeedAsync();
        await auxiliaryOperationsSeeder.SeedAsync();
        await operationsSeeder.SeedAsync();
        await productsSeeder.SeedAsync();
        await shiftsSeeder.SeedAsync();
        await shiftSchedulesSeeder.SeedAsync();
        await indicatorsSeeder.SeedAsync();
        await templatesSeeder.SeedAsync();

        await dbContext.SaveChangesAsync();

        await formsSeeder.SeedAsync();
    }
}