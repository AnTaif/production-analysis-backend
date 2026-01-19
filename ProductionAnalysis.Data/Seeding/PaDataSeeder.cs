using Core.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        var positionsSeeder = new PositionsSeeder(dbContext);
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
        await positionsSeeder.SeedAsync();
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

        await SyncSequencesAsync();

        await formsSeeder.SeedAsync();

        await SyncSequencesAsync();
    }

    private async Task SyncSequencesAsync()
    {
        var sequences = new[]
        {
            ("enterprises", "enterprises_id_seq"),
            ("departments", "departments_id_seq"),
            ("positions", "positions_id_seq"),
            ("downtime_reason_groups", "downtime_reason_groups_id_seq"),
            ("employees", "employees_id_seq"),
            ("auxiliary_operations", "auxiliary_operations_id_seq"),
            ("operations", "operations_id_seq"),
            ("products", "products_id_seq"),
            ("shifts", "shifts_id_seq"),
            ("shift_schedules", "shift_schedules_id_seq"),
            ("indicators", "indicators_id_seq"),
            ("templates", "templates_id_seq")
        };

        foreach (var (tableName, sequenceName) in sequences)
        {
            try
            {
                var sql = $@"
                    DO $$
                    DECLARE
                        max_id INTEGER;
                    BEGIN
                        -- Проверяем существование последовательности
                        IF EXISTS (SELECT 1 FROM pg_class WHERE relname = '{sequenceName}') THEN
                            -- Получаем максимальный ID из таблицы
                            SELECT COALESCE(MAX(id), 0) INTO max_id FROM {tableName};
                            
                            -- Устанавливаем значение последовательности
                            -- GREATEST гарантирует, что значение будет >= 1, даже если max_id = 0
                            PERFORM setval('{sequenceName}', GREATEST(max_id, 1), true);
                        END IF;
                    END $$;";

                await dbContext.Database.ExecuteSqlRawAsync(sql);

                logger.LogInformation("Synced sequence {SequenceName} for table {TableName}",
                    sequenceName, tableName);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to sync sequence {SequenceName} for table {TableName}. Error: {ErrorMessage}",
                    sequenceName, tableName, ex.Message);
            }
        }
    }
}