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
    // Departments
    public async Task<ICollection<DepartmentDto>> SelectDepartmentsAsync()
    {
        var dbos = await dbContext.Departments.ToListAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<DepartmentDto?> FindDepartmentByIdAsync(int departmentId)
    {
        var dbo = await dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
        return dbo?.ToDto();
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request)
    {
        var department = new DepartmentDbo
        {
            Name = request.Name,
            EnterpriseId = request.EnterpriseId
        };

        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync();

        return department.ToDto();
    }

    public async Task<DepartmentDto?> UpdateDepartmentAsync(int departmentId, UpdateDepartmentRequest request)
    {
        var department = await dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
        if (department == null)
            return null;

        department.Name = request.Name;
        department.EnterpriseId = request.EnterpriseId;

        await dbContext.SaveChangesAsync();

        return department.ToDto();
    }

    public async Task<bool> DeleteDepartmentAsync(int departmentId)
    {
        var department = await dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
        if (department == null)
            return false;

        dbContext.Departments.Remove(department);
        await dbContext.SaveChangesAsync();

        return true;
    }

    // DowntimeReasonGroups
    public async Task<ICollection<DowntimeReasonGroupDto>> SelectDowntimeReasonGroupsAsync()
    {
        var dbos = await dbContext.DowntimeReasonGroups.ToListAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<DowntimeReasonGroupDto?> FindDowntimeReasonGroupByIdAsync(int id)
    {
        var dbo = await dbContext.DowntimeReasonGroups.FirstOrDefaultAsync(d => d.Id == id);
        return dbo?.ToDto();
    }

    public async Task<DowntimeReasonGroupDto> CreateDowntimeReasonGroupAsync(CreateDowntimeReasonGroupRequest request)
    {
        var group = new DowntimeReasonGroupDbo
        {
            Name = request.Name,
            Description = request.Description
        };

        dbContext.DowntimeReasonGroups.Add(group);
        await dbContext.SaveChangesAsync();

        return group.ToDto();
    }

    public async Task<DowntimeReasonGroupDto?> UpdateDowntimeReasonGroupAsync(int id,
        UpdateDowntimeReasonGroupRequest request)
    {
        var group = await dbContext.DowntimeReasonGroups.FirstOrDefaultAsync(d => d.Id == id);
        if (group == null)
            return null;

        group.Name = request.Name;
        group.Description = request.Description;

        await dbContext.SaveChangesAsync();

        return group.ToDto();
    }

    public async Task<bool> DeleteDowntimeReasonGroupAsync(int id)
    {
        var group = await dbContext.DowntimeReasonGroups.FirstOrDefaultAsync(d => d.Id == id);
        if (group == null)
            return false;

        dbContext.DowntimeReasonGroups.Remove(group);
        await dbContext.SaveChangesAsync();

        return true;
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

    // Positions
    public async Task<ICollection<PositionDto>> SelectPositionsAsync()
    {
        var dbos = await dbContext.Positions.ToListAsync();
        return dbos.Select(p => p.ToDto()).ToList();
    }

    public async Task<PositionDto?> FindPositionByIdAsync(int positionId)
    {
        var dbo = await dbContext.Positions.FirstOrDefaultAsync(p => p.Id == positionId);
        return dbo?.ToDto();
    }

    public async Task<PositionDto> CreatePositionAsync(CreatePositionRequest request)
    {
        var position = new PositionDbo
        {
            Name = request.Name,
            Role = request.Role
        };

        dbContext.Positions.Add(position);
        await dbContext.SaveChangesAsync();

        return position.ToDto();
    }

    public async Task<PositionDto?> UpdatePositionAsync(int positionId, UpdatePositionRequest request)
    {
        var position = await dbContext.Positions.FirstOrDefaultAsync(p => p.Id == positionId);
        if (position == null)
            return null;

        position.Name = request.Name;
        position.Role = request.Role;

        await dbContext.SaveChangesAsync();

        return position.ToDto();
    }

    public async Task<bool> DeletePositionAsync(int positionId)
    {
        var position = await dbContext.Positions.FirstOrDefaultAsync(p => p.Id == positionId);
        if (position == null)
            return false;

        dbContext.Positions.Remove(position);
        await dbContext.SaveChangesAsync();

        return true;
    }

    // Enterprises
    public async Task<ICollection<EnterpriseDto>> SelectEnterprisesAsync()
    {
        var dbos = await dbContext.Enterprises.ToListAsync();
        return dbos.Select(e => e.ToDto()).ToList();
    }

    public async Task<EnterpriseDto?> FindEnterpriseByIdAsync(int enterpriseId)
    {
        var dbo = await dbContext.Enterprises.FirstOrDefaultAsync(e => e.Id == enterpriseId);
        return dbo?.ToDto();
    }

    public async Task<EnterpriseDto> CreateEnterpriseAsync(CreateEnterpriseRequest request)
    {
        var enterprise = new EnterpriseDbo
        {
            Name = request.Name
        };

        dbContext.Enterprises.Add(enterprise);
        await dbContext.SaveChangesAsync();

        return enterprise.ToDto();
    }

    public async Task<EnterpriseDto?> UpdateEnterpriseAsync(int enterpriseId, UpdateEnterpriseRequest request)
    {
        var enterprise = await dbContext.Enterprises.FirstOrDefaultAsync(e => e.Id == enterpriseId);
        if (enterprise == null)
            return null;

        enterprise.Name = request.Name;

        await dbContext.SaveChangesAsync();

        return enterprise.ToDto();
    }

    public async Task<bool> DeleteEnterpriseAsync(int enterpriseId)
    {
        var enterprise = await dbContext.Enterprises.FirstOrDefaultAsync(e => e.Id == enterpriseId);
        if (enterprise == null)
            return false;

        dbContext.Enterprises.Remove(enterprise);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EnterpriseExistsAsync(int enterpriseId)
    {
        return await dbContext.Enterprises.AnyAsync(e => e.Id == enterpriseId);
    }

    // AuxiliaryOperations
    public async Task<ICollection<AuxiliaryOperationDto>> SelectAuxiliaryOperationsAsync()
    {
        var dbos = await dbContext.AuxiliaryOperations.ToListAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<AuxiliaryOperationDto?> FindAuxiliaryOperationByIdAsync(int id)
    {
        var dbo = await dbContext.AuxiliaryOperations.FirstOrDefaultAsync(a => a.Id == id);
        return dbo?.ToDto();
    }

    public async Task<AuxiliaryOperationDto> CreateAuxiliaryOperationAsync(CreateAuxiliaryOperationRequest request)
    {
        var auxiliaryOperation = new AuxiliaryOperationDbo
        {
            Name = request.Name,
            DurationInSeconds = request.DurationInSeconds
        };

        dbContext.AuxiliaryOperations.Add(auxiliaryOperation);
        await dbContext.SaveChangesAsync();

        return auxiliaryOperation.ToDto();
    }

    public async Task<AuxiliaryOperationDto?> UpdateAuxiliaryOperationAsync(int id,
        UpdateAuxiliaryOperationRequest request)
    {
        var auxiliaryOperation = await dbContext.AuxiliaryOperations.FirstOrDefaultAsync(a => a.Id == id);
        if (auxiliaryOperation == null)
            return null;

        auxiliaryOperation.Name = request.Name;
        auxiliaryOperation.DurationInSeconds = request.DurationInSeconds;

        await dbContext.SaveChangesAsync();

        return auxiliaryOperation.ToDto();
    }

    public async Task<bool> DeleteAuxiliaryOperationAsync(int id)
    {
        var auxiliaryOperation = await dbContext.AuxiliaryOperations.FirstOrDefaultAsync(a => a.Id == id);
        if (auxiliaryOperation == null)
            return false;

        dbContext.AuxiliaryOperations.Remove(auxiliaryOperation);
        await dbContext.SaveChangesAsync();

        return true;
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

    public async Task<OperationDto?> FindOperationByIdAsync(int operationId)
    {
        var dbo = await dbContext.Operations.FirstOrDefaultAsync(o => o.Id == operationId);
        return dbo?.ToDto();
    }

    public async Task<OperationDto> CreateOperationAsync(CreateOperationRequest request)
    {
        var operation = new OperationDbo
        {
            Name = request.Name,
            DurationInSeconds = request.DurationInSeconds,
            BasedOnType = (int)request.BasedOnType,
            BasedOperationId = request.BasedOperationId,
            BasedProductId = request.BasedProductId
        };

        dbContext.Operations.Add(operation);
        await dbContext.SaveChangesAsync();

        return operation.ToDto();
    }

    public async Task<OperationDto?> UpdateOperationAsync(int operationId, UpdateOperationRequest request)
    {
        var operation = await dbContext.Operations.FirstOrDefaultAsync(o => o.Id == operationId);
        if (operation == null)
            return null;

        operation.Name = request.Name;
        operation.DurationInSeconds = request.DurationInSeconds;
        operation.BasedOnType = (int)request.BasedOnType;
        operation.BasedOperationId = request.BasedOperationId;
        operation.BasedProductId = request.BasedProductId;

        await dbContext.SaveChangesAsync();

        return operation.ToDto();
    }

    public async Task<bool> DeleteOperationAsync(int operationId)
    {
        var operation = await dbContext.Operations.FirstOrDefaultAsync(o => o.Id == operationId);
        if (operation == null)
            return false;

        dbContext.Operations.Remove(operation);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> OperationExistsAsync(int operationId)
    {
        return await dbContext.Operations.AnyAsync(o => o.Id == operationId);
    }

    // Products
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

    public async Task<ProductDto?> FindProductByIdAsync(int productId)
    {
        var dbo = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (dbo == null)
            return null;

        var productDto = dbo.ToDto();
        var allOperations = await dbContext.Operations.ToListAsync();
        var allOperationsDto = allOperations.Select(o => o.ToDto()).ToList();
        var productOperations = allOperationsDto
            .Where(o => o.BasedProductId == productDto.Id)
            .ToList();

        return productDto with { SubOperations = productOperations };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        var product = new ProductDbo
        {
            Name = request.Name,
            TactTimeInSeconds = request.TactTimeInSeconds,
            EnterpriseId = request.EnterpriseId
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return product.ToDto();
    }

    public async Task<ProductDto?> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
            return null;

        product.Name = request.Name;
        product.TactTimeInSeconds = request.TactTimeInSeconds;
        product.EnterpriseId = request.EnterpriseId;

        await dbContext.SaveChangesAsync();

        return product.ToDto();
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
            return false;

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ProductExistsAsync(int productId)
    {
        return await dbContext.Products.AnyAsync(p => p.Id == productId);
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

    public async Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request)
    {
        var shift = new ShiftDbo
        {
            Name = request.Name,
            StartTime = request.StartTime
        };

        dbContext.Shifts.Add(shift);
        await dbContext.SaveChangesAsync();

        return shift.ToDto();
    }

    public async Task<ShiftDto?> UpdateShiftAsync(int shiftId, UpdateShiftRequest request)
    {
        var shift = await dbContext.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId);
        if (shift == null)
            return null;

        shift.Name = request.Name;
        shift.StartTime = request.StartTime;

        await dbContext.SaveChangesAsync();

        return shift.ToDto();
    }

    public async Task<bool> DeleteShiftAsync(int shiftId)
    {
        var shift = await dbContext.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId);
        if (shift == null)
            return false;

        dbContext.Shifts.Remove(shift);
        await dbContext.SaveChangesAsync();

        return true;
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