using Microsoft.AspNetCore.Identity;
using ProductionAnalysis.Data.Context;
using ProductionAnalysis.Data.Models;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Application.Tests.Infrastructure;

public class TestDataBuilder(PaDbContext dbContext, UserManager<UserDbo> userManager)
{
    public async Task<UserDbo> CreateUserAsync(string email = "test@test.com", string password = "Test123!")
    {
        var user = new UserDbo
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = "test",
            LastName = "test",
            MiddleName = "test",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors)}");
        }

        return user;
    }

    public async Task<EmployeeDbo> CreateEmployeeAsync(Guid userId, int departmentId = 1)
    {
        var maxId = dbContext.Employees.Any()
            ? dbContext.Employees.Max(e => e.Id)
            : 0;

        var employee = new EmployeeDbo
        {
            Id = maxId + 1,
            UserId = userId,
            DepartmentId = departmentId,
            FirstName = "Test",
            LastName = "User",
            PositionId = 1
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        return employee;
    }

    public async Task<DepartmentDbo> CreateDepartmentAsync(int id = 1, string name = "Test Department")
    {
        await CreateEnterpriseAsync();

        var department = new DepartmentDbo
        {
            Id = id,
            Name = name,
            EnterpriseId = 1
        };

        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync();

        return department;
    }

    public async Task<ShiftDbo> CreateShiftAsync(int id = 1, TimeOnly startTime = default)
    {
        if (startTime == default)
        {
            startTime = new TimeOnly(8, 0);
        }

        var shift = new ShiftDbo
        {
            Id = id,
            Name = "Test Shift",
            StartTime = startTime
        };

        dbContext.Shifts.Add(shift);
        await dbContext.SaveChangesAsync();

        return shift;
    }

    public async Task<EnterpriseDbo> CreateEnterpriseAsync(int id = 1, string name = "Test Enterprise")
    {
        var enterprise = new EnterpriseDbo
        {
            Id = id,
            Name = name
        };

        dbContext.Enterprises.Add(enterprise);
        await dbContext.SaveChangesAsync();

        return enterprise;
    }

    public async Task<ProductDbo> CreateProductAsync(
        int id = 1,
        int enterpriseId = 1,
        string name = "Test Product",
        int tactTimeInSeconds = 5)
    {
        var product = new ProductDbo
        {
            Id = id,
            Name = name,
            TactTimeInSeconds = tactTimeInSeconds,
            EnterpriseId = enterpriseId
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return product;
    }

    public async Task<ShiftScheduleDbo> CreateShiftScheduleAsync(
        int shiftId,
        int auxiliaryOperationId,
        TimeOnly startTime)
    {
        var maxId = dbContext.ShiftSchedules.Any()
            ? dbContext.ShiftSchedules.Max(s => s.Id)
            : 0;

        var schedule = new ShiftScheduleDbo
        {
            Id = maxId + 1,
            ShiftId = shiftId,
            AuxiliaryOperationId = auxiliaryOperationId,
            StartTime = startTime
        };

        dbContext.ShiftSchedules.Add(schedule);
        await dbContext.SaveChangesAsync();

        return schedule;
    }

    public async Task<AuxiliaryOperationDbo> CreateAuxiliaryOperationAsync(
        int id = 1,
        string name = "Break",
        int durationInSeconds = 1800)
    {
        var operation = new AuxiliaryOperationDbo
        {
            Id = id,
            Name = name,
            DurationInSeconds = durationInSeconds
        };

        dbContext.AuxiliaryOperations.Add(operation);
        await dbContext.SaveChangesAsync();

        return operation;
    }

    public async Task<OperationDbo> CreateOperationAsync(
        int id,
        string name,
        int? durationInSeconds = null,
        int basedOnType = 1,
        int? basedOperationId = null,
        int? basedProductId = null)
    {
        var operation = new OperationDbo
        {
            Id = id,
            Name = name,
            DurationInSeconds = durationInSeconds,
            BasedOnType = basedOnType,
            BasedOperationId = basedOperationId,
            BasedProductId = basedProductId
        };

        dbContext.Operations.Add(operation);
        await dbContext.SaveChangesAsync();

        return operation;
    }
}