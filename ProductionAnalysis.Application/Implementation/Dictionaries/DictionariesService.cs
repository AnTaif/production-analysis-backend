using Core.Auth;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;
using Shared.Constants;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IDictionariesService
{
    Task<ICollection<DepartmentDto>> GetDepartmentsAsync();
    Task<ICollection<DowntimeReasonGroupDto>> GetDowntimeReasonGroupsAsync();
    Task<ICollection<EmployeeDto>> GetEmployeesAsync(ContextUser user);
    Task<ICollection<EnterpriseDto>> GetEnterprisesAsync();
    Task<ICollection<AuxiliaryOperationDto>> GetAuxiliaryOperationsAsync();
    Task<ICollection<OperationDto>> GetOperationsAsync();
    Task<ICollection<ProductDto>> GetProductsAsync();
    Task<ICollection<ShiftDto>> GetShiftsAsync();
}

[RegisterScoped]
public class DictionariesService(IPaUnitOfWork unitOfWork) : IDictionariesService
{
    public async Task<ICollection<DepartmentDto>> GetDepartmentsAsync()
    {
        return await unitOfWork.Dictionaries.SelectDepartmentsAsync();
    }

    public async Task<ICollection<DowntimeReasonGroupDto>> GetDowntimeReasonGroupsAsync()
    {
        return await unitOfWork.Dictionaries.SelectDowntimeReasonGroupsAsync();
    }

    public async Task<ICollection<EmployeeDto>> GetEmployeesAsync(ContextUser user)
    {
        if (user.Roles.Contains(Roles.Admin))
        {
            return await unitOfWork.Dictionaries.SelectEmployeesAsync();
        }

        var employee = await unitOfWork.Dictionaries.FindEmployeeByUserIdAsync(user.Id);
        if (employee == null)
        {
            return [];
        }

        return await unitOfWork.Dictionaries.SelectEmployeesByDepartmentIdAsync(employee.DepartmentId);
    }

    public async Task<ICollection<EnterpriseDto>> GetEnterprisesAsync()
    {
        return await unitOfWork.Dictionaries.SelectEnterprisesAsync();
    }

    public async Task<ICollection<AuxiliaryOperationDto>> GetAuxiliaryOperationsAsync()
    {
        return await unitOfWork.Dictionaries.SelectAuxiliaryOperationsAsync();
    }

    public async Task<ICollection<OperationDto>> GetOperationsAsync()
    {
        return await unitOfWork.Dictionaries.SelectOperationsAsync();
    }

    public async Task<ICollection<ProductDto>> GetProductsAsync()
    {
        return await unitOfWork.Dictionaries.SelectProductsAsync();
    }

    public async Task<ICollection<ShiftDto>> GetShiftsAsync()
    {
        return await unitOfWork.Dictionaries.SelectShiftsAsync();
    }
}