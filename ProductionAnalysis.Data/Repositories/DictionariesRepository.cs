using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Repositories;

public interface IDictionariesRepository
{
    Task<ICollection<DepartmentDbo>> SelectDepartmentsAsync();
    Task<ICollection<DowntimeReasonGroupDbo>> SelectDowntimeReasonGroupsAsync();
    Task<ICollection<EmployeeDbo>> SelectEmployeesAsync();
    Task<ICollection<EnterpriseDbo>> SelectEnterprisesAsync();
    Task<ICollection<AdditionalOperationDbo>> SelectAdditionalOperationsAsync();
    Task<ICollection<OperationDbo>> SelectOperationsAsync();
    Task<ICollection<PaTypeDbo>> SelectPaTypesAsync();
    Task<ICollection<ProductDbo>> SelectProductsAsync();
    Task<ICollection<ShiftDbo>> SelectShiftsAsync();
}

[RegisterScoped]
public class DictionariesRepository(PaDbContext dbContext) : IDictionariesRepository
{
    public async Task<ICollection<DepartmentDbo>> SelectDepartmentsAsync()
    {
        return await dbContext.Departments.ToListAsync();
    }

    public async Task<ICollection<DowntimeReasonGroupDbo>> SelectDowntimeReasonGroupsAsync()
    {
        return await dbContext.DowntimeReasonGroups.ToListAsync();
    }

    public async Task<ICollection<EmployeeDbo>> SelectEmployeesAsync()
    {
        return await dbContext.Employees.ToListAsync();
    }

    public async Task<ICollection<EnterpriseDbo>> SelectEnterprisesAsync()
    {
        return await dbContext.Enterprises.ToListAsync();
    }

    public async Task<ICollection<AdditionalOperationDbo>> SelectAdditionalOperationsAsync()
    {
        return await dbContext.AdditionalOperations.ToListAsync();
    }

    public async Task<ICollection<OperationDbo>> SelectOperationsAsync()
    {
        return await dbContext.Operations.ToListAsync();
    }

    public async Task<ICollection<PaTypeDbo>> SelectPaTypesAsync()
    {
        return await dbContext.PaTypes.ToListAsync();
    }

    public async Task<ICollection<ProductDbo>> SelectProductsAsync()
    {
        return await dbContext.Products.ToListAsync();
    }

    public async Task<ICollection<ShiftDbo>> SelectShiftsAsync()
    {
        return await dbContext.Shifts.ToListAsync();
    }
}