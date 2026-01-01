using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Converters;
using ProductionAnalysis.Data.Models;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class DictionariesRepository(
    PaDbContext dbContext,
    UserManager<UserDbo> userManager) : IDictionariesRepository
{
    public async Task<ICollection<DepartmentDto>> SelectDepartmentsAsync()
    {
        var dbos = await dbContext.Departments.ToListAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<ICollection<DowntimeReasonGroupDto>> SelectDowntimeReasonGroupsAsync()
    {
        var dbos = await dbContext.DowntimeReasonGroups.ToListAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<ICollection<EmployeeDto>> SelectEmployeesAsync()
    {
        var dbos = await dbContext.Employees.ToListAsync();
        return dbos.Select(e => e.ToDto()).ToList();
    }

    public async Task<EmployeeDto?> FindEmployeeByUserIdAsync(Guid userId)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId);

        return employee?.ToDto();
    }

    public async Task<ICollection<EnterpriseDto>> SelectEnterprisesAsync()
    {
        var dbos = await dbContext.Enterprises.ToListAsync();
        return dbos.Select(e => e.ToDto()).ToList();
    }

    public async Task<ICollection<AdditionalOperationDto>> SelectAdditionalOperationsAsync()
    {
        var dbos = await dbContext.AdditionalOperations.ToListAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<ICollection<OperationDto>> SelectOperationsAsync()
    {
        var dbos = await dbContext.Operations.ToListAsync();
        return dbos.Select(o => o.ToDto()).ToList();
    }

    public async Task<ICollection<PaTypeDto>> SelectPaTypesAsync()
    {
        var dbos = await dbContext.PaTypes.ToListAsync();
        return dbos.Select(p => p.ToDto()).ToList();
    }

    public async Task<ICollection<ProductDto>> SelectProductsAsync()
    {
        var dbos = await dbContext.Products.ToListAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<ICollection<ShiftDto>> SelectShiftsAsync()
    {
        var dbos = await dbContext.Shifts.ToListAsync();
        return dbos.Select(s => s.ToDto()).ToList();
    }

    public async Task<ShiftDto?> SelectShiftByIdAsync(int shiftId)
    {
        var dbo = await dbContext.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId);
        return dbo?.ToDto();
    }

    public async Task<ICollection<ShiftScheduleDto>> SelectShiftSchedulesByShiftIdAsync(int shiftId)
    {
        var dbos = await dbContext.ShiftSchedules
            .Where(s => s.ShiftId == shiftId)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return dbos.Select(s => s.ToDto()).ToList();
    }
}