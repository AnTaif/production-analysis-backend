using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IEmployeesService
{
    Task<Result<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeRequest request);
    Task<Result<EmployeeDto>> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request);
    Task<Result> DeleteEmployeeAsync(int employeeId);
}

[RegisterScoped]
public class EmployeesService(IPaUnitOfWork unitOfWork) : IEmployeesService
{
    public async Task<Result<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        var departmentExists = await unitOfWork.Dictionaries.DepartmentExistsAsync(request.DepartmentId);
        if (!departmentExists)
        {
            return ServiceError.NotFound($"Department with id {request.DepartmentId} not found");
        }

        var employee = await unitOfWork.Dictionaries.CreateEmployeeAsync(request);
        return employee;
    }

    public async Task<Result<EmployeeDto>> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request)
    {
        var existingEmployee = await unitOfWork.Dictionaries.FindEmployeeByIdAsync(employeeId);
        if (existingEmployee == null)
        {
            return ServiceError.NotFound($"Employee with id {employeeId} not found");
        }

        var departmentExists = await unitOfWork.Dictionaries.DepartmentExistsAsync(request.DepartmentId);
        if (!departmentExists)
        {
            return ServiceError.NotFound($"Department with id {request.DepartmentId} not found");
        }

        var updatedEmployee = await unitOfWork.Dictionaries.UpdateEmployeeAsync(employeeId, request);
        if (updatedEmployee == null)
        {
            return ServiceError.NotFound($"Employee with id {employeeId} not found");
        }

        return updatedEmployee;
    }

    public async Task<Result> DeleteEmployeeAsync(int employeeId)
    {
        var employee = await unitOfWork.Dictionaries.FindEmployeeByIdAsync(employeeId);
        if (employee == null)
        {
            return ServiceError.NotFound($"Employee with id {employeeId} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeleteEmployeeAsync(employeeId);
        if (!deleted)
        {
            return ServiceError.NotFound($"Employee with id {employeeId} not found");
        }

        return Result.Success;
    }
}