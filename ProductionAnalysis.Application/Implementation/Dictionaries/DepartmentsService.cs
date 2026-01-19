using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IDepartmentsService
{
    Task<Result<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentRequest request);
    Task<Result<DepartmentDto>> UpdateDepartmentAsync(int departmentId, UpdateDepartmentRequest request);
    Task<Result> DeleteDepartmentAsync(int departmentId);
}

[RegisterScoped]
public class DepartmentsService(IPaUnitOfWork unitOfWork) : IDepartmentsService
{
    public async Task<Result<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentRequest request)
    {
        var enterpriseExists = await unitOfWork.Dictionaries.EnterpriseExistsAsync(request.EnterpriseId);
        if (!enterpriseExists)
        {
            return ServiceError.NotFound($"Enterprise with id {request.EnterpriseId} not found");
        }

        var department = await unitOfWork.Dictionaries.CreateDepartmentAsync(request);
        return department;
    }

    public async Task<Result<DepartmentDto>> UpdateDepartmentAsync(int departmentId, UpdateDepartmentRequest request)
    {
        var existingDepartment = await unitOfWork.Dictionaries.FindDepartmentByIdAsync(departmentId);
        if (existingDepartment == null)
        {
            return ServiceError.NotFound($"Department with id {departmentId} not found");
        }

        var enterpriseExists = await unitOfWork.Dictionaries.EnterpriseExistsAsync(request.EnterpriseId);
        if (!enterpriseExists)
        {
            return ServiceError.NotFound($"Enterprise with id {request.EnterpriseId} not found");
        }

        var updatedDepartment = await unitOfWork.Dictionaries.UpdateDepartmentAsync(departmentId, request);
        if (updatedDepartment == null)
        {
            return ServiceError.NotFound($"Department with id {departmentId} not found");
        }

        return updatedDepartment;
    }

    public async Task<Result> DeleteDepartmentAsync(int departmentId)
    {
        var department = await unitOfWork.Dictionaries.FindDepartmentByIdAsync(departmentId);
        if (department == null)
        {
            return ServiceError.NotFound($"Department with id {departmentId} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeleteDepartmentAsync(departmentId);
        if (!deleted)
        {
            return ServiceError.NotFound($"Department with id {departmentId} not found");
        }

        return Result.Success;
    }
}