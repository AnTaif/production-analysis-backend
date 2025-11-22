using Microsoft.AspNetCore.Mvc;
using ProductionAnalysis.Application.Implementation.Admin;

namespace ProductionAnalysis.Api.Controllers.Admin;

[ApiController]
[Route("admin/maintenance")]
public class MaintenanceController(IDbMaintenanceService dbMaintenanceService) : ControllerBase
{
    [HttpPost("db/apply-migrations")]
    public async Task ApplyMigrations()
    {
        await dbMaintenanceService.ApplyMigrationsAsync();
    }
}