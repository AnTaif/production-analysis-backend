using Microsoft.EntityFrameworkCore;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Converters;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Repositories;

[RegisterScoped]
public class DictionariesRepository(
    PaDbContext dbContext) : IDictionariesRepository
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
        var dbos = await dbContext.Employees
            .Include(e => e.Position)
            .ToListAsync();
        return dbos.Select(e => e.ToDto()).ToList();
    }

    public async Task<ICollection<EmployeeDto>> SelectEmployeesByDepartmentIdAsync(int departmentId)
    {
        var dbos = await dbContext.Employees
            .Include(e => e.Position)
            .Where(e => e.DepartmentId == departmentId)
            .ToListAsync();
        return dbos.Select(e => e.ToDto()).ToList();
    }

    public async Task<EmployeeDto?> FindEmployeeByUserIdAsync(Guid userId)
    {
        var employee = await dbContext.Employees
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.UserId == userId);

        return employee?.ToDto();
    }

    public async Task<EmployeeDto?> FindEmployeeByIdAsync(int employeeId)
    {
        var employee = await dbContext.Employees
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        return employee?.ToDto();
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        var employee = new EmployeeDbo
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            PositionId = request.PositionId,
            Email = request.Email,
            DepartmentId = request.DepartmentId
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        // Загружаем Position для корректного маппинга
        await dbContext.Entry(employee)
            .Reference(e => e.Position)
            .LoadAsync();

        return employee.ToDto();
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request)
    {
        var employee = await dbContext.Employees
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
            return null;

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.MiddleName = request.MiddleName;
        employee.PositionId = request.PositionId;
        employee.Email = request.Email;
        employee.DepartmentId = request.DepartmentId;

        await dbContext.SaveChangesAsync();

        return employee.ToDto();
    }

    public async Task<bool> DeleteEmployeeAsync(int employeeId)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
            return false;

        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DepartmentExistsAsync(int departmentId)
    {
        return await dbContext.Departments.AnyAsync(d => d.Id == departmentId);
    }

    public async Task<bool> PositionExistsAsync(int positionId)
    {
        return await dbContext.Positions.AnyAsync(p => p.Id == positionId);
    }

    public async Task<ICollection<PositionDto>> SelectPositionsAsync()
    {
        var dbos = await dbContext.Positions.ToListAsync();
        return dbos.Select(p => p.ToDto()).ToList();
    }

    public async Task<ICollection<EnterpriseDto>> SelectEnterprisesAsync()
    {
        var dbos = await dbContext.Enterprises.ToListAsync();
        return dbos.Select(e => e.ToDto()).ToList();
    }

    public async Task<ICollection<AuxiliaryOperationDto>> SelectAuxiliaryOperationsAsync()
    {
        var dbos = await dbContext.AuxiliaryOperations.ToListAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<ICollection<OperationDto>> SelectOperationsAsync()
    {
        var allDbos = await dbContext.Operations.ToListAsync();
        var allOperations = allDbos.Select(o => o.ToDto()).ToList();

        var parentOperations = allOperations
            .Where(o => o.BasedOperationId == null && o.BasedProductId == null)
            .Select(parentOp =>
            {
                var subOperations = allOperations
                    .Where(o => o.BasedOperationId == parentOp.Id)
                    .ToList();
                return parentOp with { SubOperations = subOperations };
            })
            .ToList();

        return parentOperations;
    }

    public async Task<ICollection<OperationDto>> SelectAllOperationsAsync()
    {
        var dbos = await dbContext.Operations.ToListAsync();
        return dbos.Select(o => o.ToDto()).ToList();
    }

    public async Task<ICollection<ProductDto>> SelectProductsAsync()
    {
        var allOperations = await dbContext.Operations.ToListAsync();
        var allOperationsDto = allOperations.Select(o => o.ToDto()).ToList();

        var dbos = await dbContext.Products.ToListAsync();
        var products = dbos.Select(dbo =>
        {
            var productDto = dbo.ToDto();
            var productOperations = allOperationsDto
                .Where(o => o.BasedProductId == productDto.Id)
                .ToList();
            return productDto with { SubOperations = productOperations };
        }).ToList();

        return products;
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