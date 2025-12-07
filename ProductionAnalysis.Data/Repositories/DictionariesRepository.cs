using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Converters;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class DictionariesRepository(PaDbContext dbContext) : IDictionariesRepository
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
}